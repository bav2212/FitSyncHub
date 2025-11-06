using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using FitSyncHub.IntervalsICU.HttpClients.Models;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient
{
    public async Task<IReadOnlyCollection<EventResponse>> ListEvents(
      ListEventsQueryParams query,
      CancellationToken cancellationToken)
    {
        var queryParams = new Dictionary<string, StringValues>()
        {
            { "oldest", new DateTime(query.Oldest, TimeOnly.MinValue).ToString("s", CultureInfo.InvariantCulture) },
            { "newest", new DateTime(query.Newest, TimeOnly.MaxValue).ToString("s", CultureInfo.InvariantCulture) },
        };

        if (query.Category is not null)
        {
            var serializedCategories = query.Category
                .Select(x => JsonSerializer
                    .SerializeToElement(x, IntervalsIcuSnakeCaseSourceGenerationContext.Default.EventCategory)
                    .GetString())
                .ToList();

            queryParams.Add("category", string.Join(',', serializedCategories));
        }

        if (query.Limit is not null)
        {
            queryParams.Add("limit", query.Limit.Value.ToString(CultureInfo.InvariantCulture));
        }

        var requestUri = QueryHelpers.AddQueryString($"{AthleteBaseUrl}/events", queryParams);

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content,
            IntervalsIcuSnakeCaseSourceGenerationContext.Default.IReadOnlyCollectionEventResponse)!;
    }

    public async Task<EventResponse> GetEvent(
        int eventId,
        CancellationToken cancellationToken)
    {
        var requestUri = $"{AthleteBaseUrl}/events/{eventId}";

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuSnakeCaseSourceGenerationContext.Default.EventResponse)!;
    }

    public async Task<EventResponse> CreateEvent(
       CreateEventFromDescriptionRequest model,
       CancellationToken cancellationToken)
    {
        var requestUri = $"{AthleteBaseUrl}/events";

        var jsonContent = JsonContent.Create(model, IntervalsIcuSnakeCaseSourceGenerationContext.Default.CreateEventFromDescriptionRequest);
        var response = await _httpClient.PostAsync(requestUri, jsonContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuSnakeCaseSourceGenerationContext.Default.EventResponse)!;
    }

    public async Task<EventResponse> CreateEvent(
       CreateEventFromFileRequest model,
       CancellationToken cancellationToken)
    {
        var requestUri = $"{AthleteBaseUrl}/events";

        var jsonContent = JsonContent.Create(model, IntervalsIcuSnakeCaseSourceGenerationContext.Default.CreateEventFromFileRequest);
        var response = await _httpClient.PostAsync(requestUri, jsonContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuSnakeCaseSourceGenerationContext.Default.EventResponse)!;
    }

    public async Task DeleteEvent(
        DeleteEventRequest model,
        CancellationToken cancellationToken = default)
    {
        var notBeforeQueryParam = new DateTime(model.NotBefore, TimeOnly.MinValue)
            .ToString("s", CultureInfo.InvariantCulture);

        var queryParams = new Dictionary<string, StringValues>()
        {
            { "others", model.Others.ToString() },
            { "notBefore", notBeforeQueryParam }
        };

        var requestUri = QueryHelpers.AddQueryString(
            $"{AthleteBaseUrl}/events/{model.EventId}", queryParams);

        var response = await _httpClient.DeleteAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
