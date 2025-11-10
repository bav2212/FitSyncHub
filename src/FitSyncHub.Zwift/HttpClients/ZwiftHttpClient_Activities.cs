using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Activities;
using FitSyncHub.Zwift.JsonSerializerContexts;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.Zwift.HttpClients;

public partial class ZwiftHttpClient
{
    public async Task<IReadOnlyCollection<ZwiftActivityOverview>> ListActivities(
        long profileId,
        int start = 0,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, StringValues>
        {
            { "start", start.ToString() },
            { "limit", limit.ToString() }
        };

        var url = QueryHelpers.AddQueryString($"api/profiles/{profileId}/activities", queryParams);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content,
            ZwiftActivitiesGenerationContext.Default.IReadOnlyCollectionZwiftActivityOverview)!;
    }

    // create ZwiftActivity class if need this method
    public async Task<ZwiftActivityOverview> GetActivity(long profileId, long id, CancellationToken cancellationToken)
    {
        var url = $"api/profiles/{profileId}/activities/{id}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content, ZwiftActivitiesGenerationContext.Default.ZwiftActivityOverview)!;
    }
}
