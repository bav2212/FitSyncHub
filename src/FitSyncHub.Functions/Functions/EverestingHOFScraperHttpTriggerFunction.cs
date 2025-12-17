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
using Newtonsoft.Json.Linq;

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

    private static bool AllActiviesSynced(JsonDocument activitiesPortionJsonDocument, DateTime lastSyncedDateTime)
    {
        var activities = GetActivitiesArray(activitiesPortionJsonDocument);
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

        var feed = _everestingHOFContainer.GetItemQueryIterator<JObject>(
                    queryDefinition: query
                );

        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync(cancellationToken);
            if (response.Count > 1)
            {
                throw new Exception("Some unexpected count of items");
            }

            var item = response.Single();

            var date = item["date"];
            if (date is null || date.Type != JTokenType.String)
            {
                throw new Exception();
            }

            return DateTime.ParseExact(date.ToString(), "yyyy-MM-dd", null);
        }

        throw new Exception("Can't get last synced date time");
    }

    private static JsonDocument ExtractActivitiesJson(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var scripts = doc
            .DocumentNode
            .SelectNodes("//script")
            ?? throw new Exception("No script tags found");

        var sb = new StringBuilder();
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
            sb.Append(text);
        }

        var fullScript = sb.ToString();

        const string ActivitiesJsonStartPattern = "12:[\"$\",\"$L1c\",null,";
        var activitiesJsonStartIndex = fullScript.IndexOf(ActivitiesJsonStartPattern);

        var startFrom = activitiesJsonStartIndex + ActivitiesJsonStartPattern.Length;
        // -2 to remove ']}'
        var length = fullScript.Length - startFrom - 2;

        var json = fullScript.Substring(startFrom, length);

        return JsonDocument.Parse(json);
    }

    private static JsonElement GetActivitiesArray(JsonDocument doc)
    {
        var root = doc.RootElement;
        return root.GetProperty("activities");
    }

    private static int GetCurrentPage(JsonDocument doc)
    {
        var root = doc.RootElement;
        return root.GetProperty("currentPage").GetInt32();
    }

    private static int GetTotalPages(JsonDocument doc)
    {
        var root = doc.RootElement;
        return root.GetProperty("totalPages").GetInt32();
    }

    private async Task StoreData(
        JsonDocument doc,
        CancellationToken cancellationToken)
    {
        var activities = GetActivitiesArray(doc);

        _logger.LogInformation("Page {CurrentPage} / {TotalPages}", GetCurrentPage(doc), GetTotalPages(doc));
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
}
