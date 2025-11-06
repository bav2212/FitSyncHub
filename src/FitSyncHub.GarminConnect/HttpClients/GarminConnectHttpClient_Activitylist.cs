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

        Dictionary<string, StringValues> commonQueryParams = new()
        {
            { "startDate", $"{startDate:yyyy-MM-dd}" },
            { "endDate", $"{endDate:yyyy-MM-dd}" },
            { "limit", Limit.ToString() },
        };

        if (!string.IsNullOrEmpty(activityType))
        {
            commonQueryParams.Add("activityType", activityType);
        }

        do
        {
            Dictionary<string, StringValues> queryParams = new(commonQueryParams)
            {
                { "start", start.ToString() },
            };

            var url = QueryHelpers
                .AddQueryString("/activitylist-service/activities/search/activities", queryParams);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var array = JsonSerializer.Deserialize(content, GarminConnectActivityListSerializerContext.Default.GarminActivitySearchResponseArray);
            if (array is null || array.Length == 0)
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
