﻿using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using FitSyncHub.IntervalsICU.HttpClients.Models;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient
{
    public async Task<IReadOnlyCollection<EventResponse>> ListEvents(
      string athleteId,
      ListEventsQueryParams query,
      CancellationToken cancellationToken)
    {
        var baseUrl = $"api/v1/athlete/{athleteId}/events";

        var queryParams = new Dictionary<string, string>()
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

        var requestUri = $"{baseUrl}?{string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content,
            IntervalsIcuSnakeCaseSourceGenerationContext.Default.IReadOnlyCollectionEventResponse)!;
    }

    public async Task<EventResponse> GetEvent(
        string athleteId,
        int eventId,
        CancellationToken cancellationToken)
    {
        var requestUri = $"api/v1/athlete/{athleteId}/events/{eventId}";

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuSnakeCaseSourceGenerationContext.Default.EventResponse)!;
    }

    public async Task<EventResponse> CreateEvent(
       string athleteId,
       CreateEventRequest model,
       CancellationToken cancellationToken)
    {
        var requestUri = $"api/v1/athlete/{athleteId}/events";

        var jsonContent = JsonContent.Create(model, IntervalsIcuSnakeCaseSourceGenerationContext.Default.CreateEventRequest);
        var response = await _httpClient.PostAsync(requestUri, jsonContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuSnakeCaseSourceGenerationContext.Default.EventResponse)!;
    }

    public async Task DeleteEvent(
        string athleteId,
        int eventId,
        bool others = false,
        DateOnly? notBefore = null,
        CancellationToken cancellationToken = default)
    {
        notBefore ??= DateOnly.FromDateTime(DateTime.Today);
        var notBeforeQueryParam = new DateTime(notBefore.Value, TimeOnly.MinValue).ToString("s", CultureInfo.InvariantCulture);

        var requestUri = $"api/v1/athlete/{athleteId}/events/{eventId}?others={others}&notBefore={notBeforeQueryParam}";

        var response = await _httpClient.DeleteAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
