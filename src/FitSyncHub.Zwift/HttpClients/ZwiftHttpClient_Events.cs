using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Requests.Events;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Events;
using FitSyncHub.Zwift.JsonSerializerContexts;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.Zwift.HttpClients;

public partial class ZwiftHttpClient
{

    public async Task<List<ZwiftEventResponse>> GetEventFeedFullRangeBuggy(
        ZwiftEventFeedRequest requestModel,
        CancellationToken cancellationToken)
    {
        const string BaseUrl = "api/event-feed";
        const int MaxRequests = 100;

        var from = requestModel.From;
        // this field look buggy, maybe zwift api does not support to fiel
        var to = requestModel.To;
        var pageLimit = requestModel.PageLimit;
        var limit = requestModel.Limit;

        var ids = new HashSet<long>();
        var results = new List<ZwiftEventResponse>();

        var queryParams = new Dictionary<string, StringValues>
        {
            ["from"] = from.ToUnixTimeMilliseconds().ToString(),
            ["to"] = to.ToUnixTimeMilliseconds().ToString(),
            ["limit"] = limit.ToString(),
            // delete if need RUNNING
            ["sport"] = "CYCLING"
        };

        var pages = 0;
        var done = false;
        string? cursor = null;

        while (!done)
        {
            if (cursor != null)
            {
                queryParams["cursor"] = cursor;
            }

            var requestUri = QueryHelpers.AddQueryString(BaseUrl, queryParams);

            var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var page = JsonSerializer.Deserialize(content, ZwiftEventsGenerationContext.Default.ZwiftEventFeedResponse)!;

            if (page?.Data == null)
            {
                break;
            }

            foreach (var item in page.Data)
            {
                var ev = item.Event;
                if (ev.EventStart >= to)
                {
                    done = true;
                    break;
                }

                if (!ids.Contains(ev.Id))
                {
                    results.Add(ev);
                    ids.Add(ev.Id);
                }
            }

            if (page.Data.Count == 0 || (page.Data.Count < limit))
            {
                break;
            }

            if (pageLimit.HasValue && ++pages >= pageLimit)
            {
                break;
            }

            if (pages > MaxRequests)
            {
                _logger.LogWarning("To much pages to iterate. Stopping to avoid some block from Zwift");
                break;
            }

            cursor = page.Cursor;
        }

        return results;
    }

    public async Task<ZwiftEventResponse> GetEventFromZwfitEventViewUrl(string eventUrl, CancellationToken cancellationToken)
    {
        var url = eventUrl.Replace(
            "https://www.zwift.com/uk/events/view/",
            "api/public/events/");

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content, ZwiftEventsGenerationContext.Default.ZwiftEventResponse)!;
    }

    public async Task<string> GetEventSubgroupResults(int eventSubgroupId, CancellationToken cancellationToken)
    {
        List<JsonElement> entriesResult = [];
        const long TakeCount = 50;

        do
        {
            var url = $"api/race-results/entries?event_subgroup_id={eventSubgroupId}&limit={TakeCount}&start={entriesResult.Count}";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonDocument = JsonDocument.Parse(content);
            var entriesProperty = jsonDocument.RootElement.GetProperty("entries");
            var entriesDocuments = entriesProperty.EnumerateArray();
            entriesResult.AddRange(entriesDocuments);
        }
        while (entriesResult.Count > 0 && entriesResult.Count % TakeCount == 0);

        var mergedJson = new { entries = entriesResult }; // Create an object with "entries" key
        return JsonSerializer.Serialize(mergedJson, ZwiftEventsGenerationContext.Default.Options);
    }

    public async Task<IReadOnlyCollection<ZwiftEventSubgroupEntrantResponse>> GetEventSubgroupEntrants(
        int eventSubgroupId,
        string type = "all",
        string participation = "signed_up",
        CancellationToken cancellationToken = default)
    {
        // see sauce source code how to handle pagination
        const long TakeCount = 100;
        const long Start = 0;

        var url = $"api/events/subgroups/entrants/{eventSubgroupId}?type={type}&participation={participation}&limit={TakeCount}&start={Start}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content,
              ZwiftEventsGenerationContext.Default.IReadOnlyCollectionZwiftEventSubgroupEntrantResponse)!;
    }
}
