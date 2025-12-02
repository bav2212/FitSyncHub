using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using FitSyncHub.Zwift.HttpClients.Models.Requests.Events;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Events;
using FitSyncHub.Zwift.JsonSerializerContexts;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.Zwift.HttpClients;

public sealed partial class ZwiftHttpClient
{
    public async Task<List<ZwiftEventResponse>> GetEventFeedFullRangeBuggy(
        ZwiftEventFeedRequest requestModel,
        CancellationToken cancellationToken)
    {
        const string BaseUrl = "api/event-feed";
        const int MaxRequests = 100;

        var from = requestModel.From;
        // this field look buggy, maybe zwift api does not support 'to' field (comment from me, not sauce)
        var to = requestModel.To;
        var pageLimit = requestModel.PageLimit;
        var limit = requestModel.Limit;

        var results = new Dictionary<long, ZwiftEventResponse>();

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

            var response = await _httpClientJson.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var page = JsonSerializer.Deserialize(content,
                ZwiftEventsGenerationContext.Default.ZwiftEventFeedResponse)!;
            if (page?.Data == null)
            {
                break;
            }
            pages++;

            foreach (var item in page.Data)
            {
                var ev = item.Event;
                if (ev.EventStart > to)
                {
                    done = true;
                    break;
                }

                results.TryAdd(ev.Id, ev);
            }

            if (page.Data.Count == 0 || (page.Data.Count < limit))
            {
                break;
            }

            if (pageLimit.HasValue && pages >= pageLimit)
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

        return [.. results.Values];
    }

    public async Task<ZwiftEventResponse> GetEventFromZwfitEventViewUrl(
        string eventUrl,
        CancellationToken cancellationToken)
    {
        var match = ZwiftEventViewUrlRegex().Match(eventUrl);
        if (!match.Success)
        {
            throw new ArgumentException("Invalid Zwift event URL format.", nameof(eventUrl));
        }

        if (!long.TryParse(match.Groups[1].Value, out var zwiftEventId))
        {
            throw new ArgumentException("Invalid Zwift event ID in the URL.", nameof(eventUrl));
        }

        var baseUrl = $"api/public/events/{zwiftEventId}";

        var queryParamsString = match.Groups[2].Success
             ? match.Groups[2].Value
             : null;

        var url = string.IsNullOrEmpty(queryParamsString)
            ? baseUrl
            : $"{baseUrl}?{queryParamsString}";

        var response = await _httpClientJson.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content, ZwiftEventsGenerationContext.Default.ZwiftEventResponse)!;
    }

    public async Task<string> GetEventSubgroupResults(
        int eventSubgroupId,
        CancellationToken cancellationToken)
    {
        const string EntriesPropertyName = "entries";

        var entriesJsonArray = new JsonArray();
        var jsonObject = new JsonObject()
        {
            [EntriesPropertyName] = entriesJsonArray
        };

        const long TakeCount = 50;

        do
        {
            var url = QueryHelpers.AddQueryString("api/race-results/entries", new Dictionary<string, StringValues>
            {
                { "event_subgroup_id", eventSubgroupId.ToString() },
                { "start", entriesJsonArray.Count.ToString() },
                { "limit", TakeCount.ToString() },
            });

            var response = await _httpClientJson.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonDocument = JsonDocument.Parse(content);
            var entriesProperty = jsonDocument.RootElement.GetProperty(EntriesPropertyName);
            foreach (var entry in entriesProperty.EnumerateArray())
            {
                entriesJsonArray.Add(entry);
            }
        }
        while (entriesJsonArray.Count > 0 && entriesJsonArray.Count % TakeCount == 0);

        return jsonObject.ToJsonString(ZwiftEventsGenerationContext.Default.Options);
    }

    public async Task<IReadOnlyCollection<ZwiftEventSubgroupEntrantResponse>> GetEventSubgroupEntrants(
        int eventSubgroupId,
        string type = "all", // or 'leader', 'sweeper', 'favorite', 'following', 'other'
        string participation = "signed_up", // or 'registered',
        CancellationToken cancellationToken = default)
    {
        const long TakeCount = 100;
        const int MaxRequests = 10;

        var pages = 0;
        var done = false;

        var results = new Dictionary<long, ZwiftEventSubgroupEntrantResponse>();

        for (var start = 0; !done; start++, pages++)
        {
            if (pages >= MaxRequests)
            {
                _logger.LogWarning("To much pages to iterate for event subgroup entrants. Stopping to avoid some block from Zwift");
                break;
            }

            var url = QueryHelpers.AddQueryString($"api/events/subgroups/entrants/{eventSubgroupId}", new Dictionary<string, StringValues>
            {
                { "type", type },
                { "participation", participation },
                { "start", start.ToString() },
                { "limit", TakeCount.ToString() },
            });

            var response = await _httpClientJson.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var page = JsonSerializer.Deserialize(content,
                  ZwiftEventsGenerationContext.Default.IReadOnlyCollectionZwiftEventSubgroupEntrantResponse)!;

            foreach (var item in page)
            {
                results.TryAdd(item.Id, item);
            }

            if (page.Count == 0 || (page.Count < TakeCount))
            {
                done = true;
            }
        }

        return [.. results.Values];
    }

    [GeneratedRegex(@"events/view/(\d+)(?:\?(.*))?")]
    private static partial Regex ZwiftEventViewUrlRegex();
}
