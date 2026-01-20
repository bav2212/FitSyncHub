using System.Buffers;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using FitSyncHub.Common.Collections;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.Functions.Functions;

public sealed class EverestingHOFScraperHttpTriggerFunction
{
    private readonly HttpClient _httpClient;
    private readonly Container _everestingHOFContainer;
    private readonly ILogger<EverestingHOFScraperHttpTriggerFunction> _logger;

    private readonly HashSet<string> _availableModalities =
        ["quarter", "full", "roam", "triple", "half", "10k", "double"];

    public EverestingHOFScraperHttpTriggerFunction(
        HttpClient httpClient,
        CosmosClient cosmosClient,
        ILogger<EverestingHOFScraperHttpTriggerFunction> logger)
    {
        _httpClient = httpClient;
        _everestingHOFContainer = cosmosClient.GetDatabase("fit-sync-hub").GetContainer("EverestingHOF");

        _logger = logger;
    }

#if DEBUG
    [Function(nameof(EverestingHOFScraperHttpTriggerFunction))]
#endif
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "everesting-hof-scraper")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        const string BaseUrl = "https://hof.everesting.com/activities";
        var lastSyncedDateTime = await GetLastSynchronizedDate(cancellationToken);

        var page = 1;
        int? totalPages = default;
        var latestUpsertResults = new FixedSizeQueue<UpsertResult>(5 * 5);

        do
        {
            var url = QueryHelpers.AddQueryString(BaseUrl, new Dictionary<string, StringValues>
                {
                    {"order", "date_desc" },
                    {"page", page.ToString() },
                });

            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var activitiesPortionJsonDocument = ExtractActivitiesJson(content);
                if (!totalPages.HasValue)
                {
                    totalPages = GetTotalPages(activitiesPortionJsonDocument);
                }

                var upsertResults = await UpsertActivities(activitiesPortionJsonDocument, cancellationToken);
                latestUpsertResults.Enqueue(upsertResults);

                var allActiviesSynced = AllActiviesSynced(activitiesPortionJsonDocument, lastSyncedDateTime);
                if (allActiviesSynced
                    && latestUpsertResults.All(x => x.Error is null && x.Status == UpsertStatus.Updated)
                    && latestUpsertResults.Count == latestUpsertResults.MaxSize)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                _ = ex;
                throw;
            }
            finally
            {
                page++;
            }
        }
        while (page <= totalPages);

        return new OkObjectResult("Success");
    }

    private static bool AllActiviesSynced(
        JsonElement root,
        DateTime lastSyncedDateTime)
    {
        var activities = GetActivitiesArray(root);
        var dtos = activities.EnumerateArray()
            .Select(x => x.Deserialize<ActivityItemProjection>())
            .ToList();

        return dtos.Count != 0
            && dtos.All(x => x is not null && x.Date != default && x.Date < lastSyncedDateTime);
    }

    private async Task<DateTime> GetLastSynchronizedDate(CancellationToken cancellationToken)
    {
        var feed = _everestingHOFContainer.GetItemLinqQueryable<ActivityItemProjection>()
            .OrderByDescending(x => x.Date)
            .Take(1)
            .ToFeedIterator();

        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync(cancellationToken);
            var item = response.Single();
            return item.Date;
        }

        throw new Exception("Can't get last synced date time");
    }

    private static JsonElement ExtractActivitiesJson(string html)
    {
        var scriptParts = GetScriptParts(html);
        return ParseActivitiesJsonFromFullScript(scriptParts);
    }

    private static IEnumerable<string> GetScriptParts(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var scripts = doc.DocumentNode.SelectNodes("//script")
            ?? throw new Exception("No script tags found");

        foreach (var script in scripts)
        {
            var text = script.InnerText;
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (!text.Contains("self.__next_f.push([1,\""))
            {
                continue;
            }

            text = text
                .Replace("self.__next_f.push([1,\"", "")
                .Replace("\"])", "");

            text = Regex.Unescape(text);
            yield return text;
        }
    }

    private static JsonElement ParseActivitiesJsonFromFullScript(IEnumerable<string> scripts)
    {
        var writer = new ArrayBufferWriter<byte>();
        const string StartPattern = "12:[\"$\",\"$L1c\",null,";

        foreach (var script in scripts)
        {
            if (writer.WrittenCount == 0)
            {
                var idx = script.IndexOf(StartPattern);
                if (idx < 0)
                {
                    continue;
                }

                writer.Write(Encoding.UTF8.GetBytes(script[(idx + StartPattern.Length)..]));
            }
            else
            {
                writer.Write(Encoding.UTF8.GetBytes(script));
            }

            //isFinalBlock: false to avoid exception throwing
            var reader = new Utf8JsonReader(writer.WrittenSpan, isFinalBlock: false, new());
            if (JsonDocument.TryParseValue(ref reader, out var document))
            {
                return document.RootElement;
            }

            // Not enough data yet — continue
        }

        throw new InvalidOperationException("no json");
    }

    private static JsonElement GetActivitiesArray(JsonElement root) => root.GetProperty("activities");
    private static int GetCurrentPage(JsonElement root) => root.GetProperty("currentPage").GetInt32();
    private static int GetTotalPages(JsonElement root) => root.GetProperty("totalPages").GetInt32();

    private async Task<List<UpsertResult>> UpsertActivities(
        JsonElement root,
        CancellationToken cancellationToken)
    {
        var activities = GetActivitiesArray(root);

        _logger.LogInformation("Page {CurrentPage} / {TotalPages}", GetCurrentPage(root), GetTotalPages(root));
        _logger.LogInformation("Activities: {ActivitiesCount}", activities.GetArrayLength());

        var tasks = new List<Task<ResponseMessage>>();
        foreach (var activity in activities.EnumerateArray())
        {
            // Convert JsonElement → stream
            var stream = new MemoryStream(); //do not dispose, will be disposed after SDK read
            await using (var writer = new Utf8JsonWriter(stream))
            {
                activity.WriteTo(writer);
            }

            stream.Position = 0;

            var id = activity.GetProperty("id").GetString()!;

            // Upsert in bulk — SDK may read/clone the stream asynchronously, so do not dispose it here
            tasks.Add(TaskToUpsertItemAndCloseStream(stream, id, cancellationToken));

            async Task<ResponseMessage> TaskToUpsertItemAndCloseStream(MemoryStream stream, string id, CancellationToken cancellationToken)
            {
                try
                {
                    return await _everestingHOFContainer.UpsertItemStreamAsync(
                        stream,
                        new PartitionKey(id),
                        cancellationToken: cancellationToken);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    await stream.DisposeAsync();
                }
            }
        }

        var result = new List<UpsertResult>();

        await foreach (var task in Task.WhenEach(tasks))
        {
            UpsertResult upsertResult;

            var response = await task;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Updated item successfully.");
                upsertResult = new UpsertResult { Status = UpsertStatus.Updated };
            }
            else if (response.StatusCode == HttpStatusCode.Created)
            {
                _logger.LogInformation("Created item successfully.");
                upsertResult = new UpsertResult { Status = UpsertStatus.Created };
            }
            else
            {
                _logger.LogError("Failed to upsert item. Status code: {StatusCode}", response.StatusCode);
                upsertResult = new UpsertResult { Error = response.ErrorMessage };
            }

            result.Add(upsertResult);
        }

        return result;
    }

    private record ActivityItemProjection
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; init; }
    }

    private record UpsertResult
    {
        public UpsertStatus? Status { get; init; }
        public string? Error { get; init; }
    }

    private enum UpsertStatus
    {
        Created,
        Updated
    }
}
