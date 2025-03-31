using System.Net.Http.Json;
using System.Text.Json;
using FitSyncHub.Functions.JsonSerializerContexts;
using FitSyncHub.GarminConnect.Models.Requests;
using FitSyncHub.GarminConnect.Models.Responses;

namespace FitSyncHub.GarminConnect.HttpClients;

public partial class GarminConnectHttpClient
{
    private readonly HttpClient _httpClient;

    public GarminConnectHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<GarminActivityResponse>> GetActivitiesByDate(DateTime startDate,
        DateTime endDate,
        string? activityType,
        CancellationToken cancellationToken = default)
    {
        var start = 0;
        const int Limit = 20;
        var activitySlug = string.IsNullOrEmpty(activityType) ? "" : "&activityType=" + activityType;
        List<GarminActivityResponse> result = [];

        do
        {
            var url = $"/activitylist-service/activities/search/activities?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&start={start}&limit={Limit}{activitySlug}";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var array = JsonSerializer.Deserialize(content, GarminConnectCamelCaseSerializerContext.Default.GarminActivityResponseArray) ?? [];
            if (array.Length == 0)
            {
                break;
            }

            result.AddRange(array);
            start += Limit;
        }
        while (result.Count % Limit == 0);

        return result.AsReadOnly();
    }

    public async Task<HttpResponseMessage> UpdateActivity(
        GarminActivityUpdateRequest model,
        CancellationToken cancellationToken = default)
    {
        var url = $"/activity-service/activity/{model.ActivityId}";

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("X-Http-Method-Override", "PUT");
        request.Content = JsonContent.Create(model,
            GarminConnectCamelCaseSerializerContext.Default.GarminActivityUpdateRequest);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        return response;
    }
}
