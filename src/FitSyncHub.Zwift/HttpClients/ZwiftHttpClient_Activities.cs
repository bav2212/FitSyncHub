using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Activities;
using FitSyncHub.Zwift.JsonSerializerContexts;

namespace FitSyncHub.Zwift.HttpClients;

public partial class ZwiftHttpClient
{
    public async Task<IReadOnlyCollection<ZwiftActivityOverview>> ListActivities(
        long profileId,
        int start = 0,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/profiles/{profileId}/activities?start={start}&limit={limit}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content,
            ZwiftActivitiesGenerationContext.Default.IReadOnlyCollectionZwiftActivityOverview)!;
    }

    // create ZwiftActivity class if need this method
    public async Task<ZwiftActivityOverview> GetActivity(long profileId, long id, CancellationToken cancellationToken)
    {
        var url = $"/api/profiles/{profileId}/activities/{id}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content, ZwiftActivitiesGenerationContext.Default.ZwiftActivityOverview)!;
    }
}
