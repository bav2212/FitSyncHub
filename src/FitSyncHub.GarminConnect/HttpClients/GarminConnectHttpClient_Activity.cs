using System.Net.Http.Json;
using System.Text.Json;
using FitSyncHub.GarminConnect.JsonSerializerContexts;
using FitSyncHub.GarminConnect.Models.Requests;
using FitSyncHub.GarminConnect.Models.Responses;

namespace FitSyncHub.GarminConnect.HttpClients;

public partial class GarminConnectHttpClient
{
    public async Task<GarminActivityResponse> GetActivity(
        long activityId,
        CancellationToken cancellationToken = default)
    {
        var url = $"/activity-service/activity/{activityId}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(content,
            GarminConnectActivitySerializerContext.Default.GarminActivityResponse)!;
    }

    public async Task<HttpResponseMessage> UpdateActivity(
        GarminActivityUpdateRequest model,
        CancellationToken cancellationToken = default)
    {
        var url = $"/activity-service/activity/{model.ActivityId}";

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("X-Http-Method-Override", "PUT");
        request.Content = JsonContent.Create(model,
            GarminConnectActivitySerializerContext.Default.GarminActivityUpdateRequest);

        return await _httpClient.SendAsync(request, cancellationToken);
    }

    public async Task<Stream> DownloadActivityFile(
      long activityId,
      CancellationToken cancellationToken = default)
    {
        var url = $"/download-service/files/activity/{activityId}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}
