using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Cosmos;
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
        var lastSyncedDateTime = await GetLastSynchronizedDate(cancellationToken);

        // do not know how HOF works, add this delta to be sure that we synced all
        // maybe need to increase days
        lastSyncedDateTime = lastSyncedDateTime.AddDays(-5);

        const string BaseUrl = "https://hof.everesting.com/activities";

        var page = 1;
        int? totalPages = default;

        do
        {
            var url = QueryHelpers.AddQueryString(BaseUrl, new Dictionary<string, StringValues>
                {
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

                if (AllActiviesSynced(activitiesPortionJsonDocument, lastSyncedDateTime))
                {
                    break;
                }

                await StoreData(activitiesPortionJsonDocument, cancellationToken);
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
        HashSet<DateTime> dates = [];

        foreach (var activity in activities.EnumerateArray())
        {
            var date = activity.GetProperty("date");
            var dateParsed = DateTime.ParseExact(date.ToString(), "yyyy-MM-dd", null);
            dates.Add(dateParsed);
        }

        return dates.All(d => d < lastSyncedDateTime);
    }

    private async Task<DateTime> GetLastSynchronizedDate(CancellationToken cancellationToken)
    {
        var query = new QueryDefinition(
            """
            SELECT top 1 * FROM c
            ORDER BY c.date DESC
            """);

        var feed = _everestingHOFContainer.GetItemQueryIterator<ActivityItemProjection>(
            queryDefinition: query
        );

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

    private async Task StoreData(
        JsonElement root,
        CancellationToken cancellationToken)
    {
        var activities = GetActivitiesArray(root);

        _logger.LogInformation("Page {CurrentPage} / {TotalPages}", GetCurrentPage(root), GetTotalPages(root));
        _logger.LogInformation("Activities: {ActivitiesCount}", activities.GetArrayLength());

        var tasks = new List<Task>();
        foreach (var activity in activities.EnumerateArray())
        {
            // Convert JsonElement → stream
            await using var stream = new MemoryStream();
            await using (var writer = new Utf8JsonWriter(stream))
            {
                activity.WriteTo(writer);
            }

            stream.Position = 0;

            // Upsert in bulk
            tasks.Add(_everestingHOFContainer.UpsertItemStreamAsync(
                stream,
                new PartitionKey(activity.GetProperty("id").GetString()),
                cancellationToken: cancellationToken)
            );
        }

        // Fire all requests in parallel
        await Task.WhenAll(tasks);
    }

    private record ActivityItemProjection
    {
        public DateTime Date { get; init; }
    }
}
