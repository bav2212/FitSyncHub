using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using FitSyncHub.Common.Helpers;
using FitSyncHub.Common.Models;
using FitSyncHub.IntervalsICU.HttpClients.Models;
using FitSyncHub.IntervalsICU.HttpClients.Models.Common;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient
{
    public async Task<IReadOnlyCollection<ActivityResponse?>> ListActivities(
        ListActivitiesQueryParams query,
        CancellationToken cancellationToken)
    {
        var queryParams = new Dictionary<string, StringValues>()
        {
            { "oldest", query.Oldest.ToString("s", CultureInfo.InvariantCulture) },
            { "newest", query.Newest.ToString("s", CultureInfo.InvariantCulture) },
            { "limit",  query.Limit.ToString() },
        };

        var requestUri = QueryHelpers.AddQueryString($"{AthleteBaseUrl}/activities", queryParams);

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;

        return [.. FilterStravaActivitiesAndDeserialize(content)];
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

    public async Task UpdateActivity(
       string activityId,
       ActivityUpdateRequest model,
       CancellationToken cancellationToken)
    {
        var requestUri = $"activity/{activityId}";

        var jsonContent = JsonContent.Create(model, IntervalsActivityUpdateSourceGenerationContext.Default.ActivityUpdateRequest);
        var response = await _httpClient.PutAsync(requestUri, jsonContent, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<ActivityCreateResponse> CreateActivity(
        FileModel fileModel,
        CreateActivityRequest createActivityRequest,
        CancellationToken cancellationToken = default)
    {
        var requestUri = $"{AthleteBaseUrl}/activities";

        using var formData = FormDataContentHelper.CreateMultipartFormDataContent(
            fileModel, createActivityRequest, IntervalsIcuSnakeCaseSourceGenerationContext.Default.CreateActivityRequest);

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

    private static IEnumerable<ActivityResponse?> FilterStravaActivitiesAndDeserialize(string content)
    {
        var jsonDocument = JsonDocument.Parse(content);

        foreach (var jsonObject in jsonDocument.RootElement.EnumerateArray())
        {
            var activitySource = jsonObject.GetProperty("source")
                .Deserialize(IntervalsIcuSnakeCaseSourceGenerationContext.Default.ActivitySource)!;
            if (activitySource == ActivitySource.Strava)
            {
                // return null for strava activities, but we need it in pagination
                yield return null; // Skip Strava activities
                continue;
            }

            yield return jsonObject.Deserialize(IntervalsIcuSnakeCaseSourceGenerationContext.Default.ActivityResponse)!;
        }
    }
}
