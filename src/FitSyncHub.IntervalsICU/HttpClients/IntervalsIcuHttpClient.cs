using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;
using ZwiftToIntervalsICUConverter.HttpClients.Models;

namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

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
        return JsonSerializer.Deserialize(content, IntervalsIcuSourceGenerationContext.Default.IReadOnlyCollectionActivityResponse)!;
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
        return JsonSerializer.Deserialize(content, IntervalsIcuSourceGenerationContext.Default.ActivityResponse)!;
    }

    public async Task UpdateActivity(
       string activityId,
       ActivityUpdateRequest model,
       CancellationToken cancellationToken)
    {
        var requestUri = $"api/v1/activity/{activityId}";

        var jsonContent = JsonContent.Create(model, IntervalsIcuSourceGenerationContext.Default.ActivityUpdateRequest);
        var response = await _httpClient.PutAsync(requestUri, jsonContent, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<ActivityCreateResponse> CreateActivity(
       string athleteId,
       byte[] activityBytes,
       string? name = default,
       string? description = default,
       int? pairedEventId = default,
       CancellationToken cancellationToken = default)
    {
        var requestUri = $"api/v1/athlete/{athleteId}/activities";

        using var formData = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(activityBytes);

        // Add the file to the form-data with the key 'file'
        formData.Add(fileContent, "file", "merged.fit");
        if (name != null)
        {
            formData.Add(new StringContent(name), "name");
        }

        if (description != null)
        {
            formData.Add(new StringContent(description), "description");
        }

        if (pairedEventId.HasValue)
        {
            formData.Add(new StringContent(pairedEventId.Value.ToString()), "paired_event_id");
        }

        // Send POST request
        var response = await _httpClient.PostAsync(requestUri, formData, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuSourceGenerationContext.Default.ActivityCreateResponse)!;
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
