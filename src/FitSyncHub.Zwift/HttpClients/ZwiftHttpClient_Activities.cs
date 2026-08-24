using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Requests.Activities;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Activities;
using FitSyncHub.Zwift.JsonSerializerContexts;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.Zwift.HttpClients;

public sealed partial class ZwiftHttpClient
{
    public async Task<IReadOnlyCollection<ZwiftActivityOverview>> ListActivities(
        ZwiftListActivitiesRequest query,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, StringValues>
        {
            { "start", query.Start.ToString() },
            { "limit", query.Limit.ToString() }
        };

        var url = QueryHelpers.AddQueryString($"api/profiles/{query.ProfileId}/activities", queryParams);

        var response = await _httpClientJson.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content,
            ZwiftHttpClientActivitiesGenerationContext.Default.IReadOnlyCollectionZwiftActivityOverview)!;
    }

    // create ZwiftActivity sealed class if need this method
    public async Task<ZwiftActivityOverview> GetActivity(
        long profileId,
        long id,
        CancellationToken cancellationToken)
    {
        var url = $"api/profiles/{profileId}/activities/{id}";

        var response = await _httpClientJson.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content,
            ZwiftHttpClientActivitiesGenerationContext.Default.ZwiftActivityOverview)!;
    }
}
