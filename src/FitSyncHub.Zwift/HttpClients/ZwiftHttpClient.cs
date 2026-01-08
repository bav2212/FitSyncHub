using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using FitSyncHub.Zwift.JsonSerializerContexts;
using FitSyncHub.Zwift.Protobuf;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Zwift.HttpClients;

public sealed partial class ZwiftHttpClient
{
    private readonly HttpClient _httpClientJson;
    private readonly HttpClient _httpClientProto;
    private readonly ILogger<ZwiftHttpClient> _logger;

    public ZwiftHttpClient(IHttpClientFactory httpClientFactory, ILogger<ZwiftHttpClient> logger)
    {
        _httpClientJson = httpClientFactory.CreateClient(Constants.ZwiftHttpClientJson);
        _httpClientProto = httpClientFactory.CreateClient(Constants.ZwiftHttpClientProto);
        _logger = logger;
    }

    public async Task<List<int>> GetAchievements(CancellationToken cancellationToken)
    {
        const string Url = "achievement/loadPlayerAchievements";

        var response = await _httpClientProto.GetAsync(Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var achievements = Achievements.Parser.ParseFrom(stream);

        return [.. achievements.Achievements_.Select(x => x.Id)];
    }

    public async Task<ZwiftGameInfoResponse> GetGameInfo(CancellationToken cancellationToken)
    {
        const string Url = "api/game_info";

        var response = await _httpClientJson.GetAsync(Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content,
            ZwiftHttpClientGameInfoGenerationContext.Default.ZwiftGameInfoResponse)!;
    }
}
