using System.Text.Json;
using FitSyncHub.GarminConnect.JsonSerializerContexts;
using FitSyncHub.GarminConnect.Models.Responses;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.GarminConnect.HttpClients;

public partial class GarminConnectHttpClient
{
    public async Task<IReadOnlyCollection<GarminActivitySearchResponse>> GetActivitiesByDate(DateTime startDate,
        DateTime endDate,
        string? activityType = default,
        CancellationToken cancellationToken = default)
    {
        var start = 0;
        const int Limit = 20;
        List<GarminActivitySearchResponse> result = [];

        do
        {
            Dictionary<string, StringValues> queryParams = new()
            {
                { "startDate", startDate.ToString("yyyy-MM-dd") },
                { "endDate", endDate.ToString("yyyy-MM-dd") },
                { "start", start.ToString() },
                { "limit", Limit.ToString() },
            };

            if (!string.IsNullOrEmpty(activityType))
            {
                queryParams.Add("activityType", activityType);
            }

            var url = QueryHelpers.AddQueryString("/activitylist-service/activities/search/activities", queryParams);

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
