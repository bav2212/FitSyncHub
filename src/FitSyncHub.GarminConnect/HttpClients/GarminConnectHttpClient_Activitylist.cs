using System.Text.Json;
using FitSyncHub.GarminConnect.JsonSerializerContexts;
using FitSyncHub.GarminConnect.Models.Responses;

namespace FitSyncHub.GarminConnect.HttpClients;

public partial class GarminConnectHttpClient
{
    public async Task<IReadOnlyCollection<GarminActivitySearchResponse>> GetActivitiesByDate(DateTime startDate,
        DateTime endDate,
        string? activityType,
        CancellationToken cancellationToken = default)
    {
        var start = 0;
        const int Limit = 20;
        var activitySlug = string.IsNullOrEmpty(activityType) ? "" : "&activityType=" + activityType;
        List<GarminActivitySearchResponse> result = [];

        do
        {
            var url = $"/activitylist-service/activities/search/activities?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&start={start}&limit={Limit}{activitySlug}";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var array = JsonSerializer.Deserialize(content, GarminConnectActivityListSerializerContext.Default.GarminActivitySearchResponseArray) ?? [];
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
}
