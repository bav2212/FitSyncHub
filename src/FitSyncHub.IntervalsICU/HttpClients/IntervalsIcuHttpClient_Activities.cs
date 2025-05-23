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

namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient
{
    public async Task<IReadOnlyCollection<ActivityResponse>> ListActivities(
        string athleteId,
        DateTime oldest,
        DateTime newest,
        int limit,
        CancellationToken cancellationToken)
    {
        var baseUrl = $"api/v1/athlete/{athleteId}/activities";

        var queryParams = new Dictionary<string, string>()
        {
            { "oldest", oldest.ToString("s", CultureInfo.InvariantCulture) },
            { "newest", newest.ToString("s", CultureInfo.InvariantCulture) },
            { "limit",  limit.ToString() },
        };

        var requestUri = $"{baseUrl}?{string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;

        return [.. FilterStravaActivitiesAndDeserialize(content)];
    }

    private static IEnumerable<ActivityResponse> FilterStravaActivitiesAndDeserialize(string content)
    {
        var jsonDocument = JsonDocument.Parse(content);

        foreach (var jsonObject in jsonDocument.RootElement.EnumerateArray())
        {
            var activitySource = jsonObject.GetProperty("source")
                .Deserialize(IntervalsIcuSnakeCaseSourceGenerationContext.Default.ActivitySource)!;
            if (activitySource == ActivitySource.Strava)
            {
                continue;
            }

            yield return jsonObject.Deserialize(IntervalsIcuSnakeCaseSourceGenerationContext.Default.ActivityResponse)!;
        }
    }

    public async Task<Stream> DownloadOriginalActivityFitFile(
       string activityId,
       CancellationToken cancellationToken)
    {
        var requestUri = $"api/v1/activity/{activityId}/file";

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public async Task UnlinkPairedWorkout(
       string activityId,
       CancellationToken cancellationToken)
    {
        var requestUri = $"api/v1/activity/{activityId}";
        var content = new StringContent("""{ "paired_event_id":0}""", MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json));

        var response = await _httpClient.PutAsync(requestUri, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<ActivityResponse> GetActivity(
      string activityId,
      CancellationToken cancellationToken)
    {
        var requestUri = $"api/v1/activity/{activityId}";

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
        var requestUri = $"api/v1/activity/{activityId}";

        var jsonContent = JsonContent.Create(model, IntervalsActivityUpdateSourceGenerationContext.Default.ActivityUpdateRequest);
        var response = await _httpClient.PutAsync(requestUri, jsonContent, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<ActivityCreateResponse> CreateActivity(
       string athleteId,
       FileModel fileModel,
       CreateActivityRequest createActivityRequest,
       CancellationToken cancellationToken = default)
    {
        var requestUri = $"api/v1/athlete/{athleteId}/activities";

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
        var requestUri = $"api/v1/activity/{activityId}";

        var response = await _httpClient.DeleteAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
