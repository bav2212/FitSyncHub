using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using FitSyncHub.Common.Helpers;
using FitSyncHub.Common.Models;
using FitSyncHub.IntervalsICU.Helpers;
using FitSyncHub.IntervalsICU.HttpClients.Models;
using FitSyncHub.IntervalsICU.HttpClients.Models.Common;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient
{
    public Task<IReadOnlyCollection<ActivityResponse?>> ListActivities(
        ActivityListQueryParams query,
        CancellationToken cancellationToken)
    {
        return ListActivitiesCore(query, fields: null,
                static row => row.Deserialize(IntervalsIcuSnakeCaseSourceGenerationContext.Default.ActivityResponse),
                cancellationToken);
    }

    public Task<IReadOnlyCollection<T?>> ListActivities<T>(
        ActivityListQueryParams query,
        Expression<Func<ActivityResponse, T>> selector,
        CancellationToken cancellationToken)
        where T : class
    {
        var (fields, deserialize) = ActivityResponseProjection.Create(selector);
        return ListActivitiesCore(query, fields, deserialize, cancellationToken);
    }

    private async Task<IReadOnlyCollection<T?>> ListActivitiesCore<T>(
        ActivityListQueryParams query,
        string? fields,
        Func<JsonElement, T?> deserialize,
        CancellationToken cancellationToken)
        where T : class
    {
        var queryParams = new Dictionary<string, StringValues>()
        {
            { "oldest", $"{query.Oldest:s}" },
            { "newest", $"{query.Newest:s}" },
            { "limit",  query.Limit.ToString() },
        };

        if (fields is not null)
        {
            queryParams.Add("fields", fields);
        }

        var requestUri = QueryHelpers.AddQueryString($"{AthleteBaseUrl}/activities", queryParams);

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return DeserializeActivities(content, deserialize);
    }

    public async Task<ActivityResponse> GetActivity(
        string activityId,
        CancellationToken cancellationToken)
    {
        var requestUri = $"activity/{activityId}";

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuSnakeCaseSourceGenerationContext.Default.ActivityResponse)!;
    }

    public async Task<ActivityResponse> UpdateActivity(
       string activityId,
       ActivityUpdateRequest model,
       CancellationToken cancellationToken)
    {
        var requestUri = $"activity/{activityId}";

        var jsonContent = JsonContent.Create(model, IntervalsUpdateSourceGenerationContext.Default.ActivityUpdateRequest);
        var response = await _httpClient.PutAsync(requestUri, jsonContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(content, IntervalsIcuSnakeCaseSourceGenerationContext.Default.ActivityResponse)!;
    }

    public async Task<ActivityCreateResponse> CreateActivity(
        FileModel fileModel,
        ActivityCreateRequest createActivityRequest,
        CancellationToken cancellationToken)
    {
        var requestUri = $"{AthleteBaseUrl}/activities";

        using var formData = FormDataContentHelper.CreateMultipartFormDataContent(
            fileModel, createActivityRequest, IntervalsIcuSnakeCaseSourceGenerationContext.Default.ActivityCreateRequest);

        // Send POST request
        var response = await _httpClient.PostAsync(requestUri, formData, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuSnakeCaseSourceGenerationContext.Default.ActivityCreateResponse)!;
    }

    public async Task DeleteActivity(
         string activityId,
         CancellationToken cancellationToken)
    {
        var requestUri = $"activity/{activityId}";

        var response = await _httpClient.DeleteAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Stream> DownloadOriginalActivityFitFile(
       string activityId,
       CancellationToken cancellationToken)
    {
        var requestUri = $"activity/{activityId}/file";

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public async Task UnlinkPairedWorkout(
       string activityId,
       CancellationToken cancellationToken)
    {
        var requestUri = $"activity/{activityId}";
        var content = new StringContent("""{ "paired_event_id":0}""", MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json));

        var response = await _httpClient.PutAsync(requestUri, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<ActivityStream>> ListStreamsForActivity(
        string activityId,
        ActivityStreamsListQueryParams query,
        CancellationToken cancellationToken)
    {
        var queryParams = new Dictionary<string, StringValues>();
        if (query.Types is not null)
        {
            queryParams.Add("types", string.Join(",", query.Types));
        }

        if (query.IncludeDefaults.HasValue)
        {
            queryParams.Add("includeDefaults", $"{query.IncludeDefaults.Value}");
        }

        var requestUri = QueryHelpers.AddQueryString($"activity/{activityId}/streams", queryParams);

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsActivityStreamsSourceGenerationContext.Default.ListActivityStream)!;
    }

    public async Task UpdateStreamsForActivity(
        string activityId,
        List<ActivityStream> model,
        CancellationToken cancellationToken)
    {
        var requestUri = $"activity/{activityId}/streams";

        var jsonContent = JsonContent.Create(model, IntervalsActivityStreamsSourceGenerationContext.Default.ListActivityStream);
        var response = await _httpClient.PutAsync(requestUri, jsonContent, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static IReadOnlyCollection<T?> DeserializeActivities<T>(
        string content, Func<JsonElement, T?> deserialize)
        where T : class
    {
        using var jsonDocument = JsonDocument.Parse(content);
        var rows = jsonDocument.RootElement;
        var activities = new List<T?>(rows.GetArrayLength());

        foreach (var row in rows.EnumerateArray())
        {
            var activitySource = row.GetProperty("source")
                .Deserialize(IntervalsIcuSnakeCaseSourceGenerationContext.Default.ActivitySource);
            // Keep Strava placeholders so the count still reflects the full page.
            activities.Add(activitySource == ActivitySource.Strava ? null : deserialize(row));
        }

        return activities;
    }
}
