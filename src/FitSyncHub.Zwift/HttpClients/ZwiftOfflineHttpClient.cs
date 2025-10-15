using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftOffline;
using FitSyncHub.Zwift.JsonSerializerContexts;

namespace FitSyncHub.Zwift.HttpClients;

public class ZwiftOfflineHttpClient
{
    private readonly HttpClient _httpClient;

    public ZwiftOfflineHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ZwiftDataGameInfoResponse> GetGameInfo(CancellationToken cancellationToken)
    {
        var gameInfoResponse = await _httpClient.GetAsync(
            "https://raw.githubusercontent.com/zoffline/zwift-offline/master/data/game_info.txt",
            cancellationToken);
        gameInfoResponse.EnsureSuccessStatusCode();

        var content = await gameInfoResponse.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(content,
            ZwiftOfflineGenerationContext.Default.ZwiftDataGameInfoResponse)!;
    }
}
