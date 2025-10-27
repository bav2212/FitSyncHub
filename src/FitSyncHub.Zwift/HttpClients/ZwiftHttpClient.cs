using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using FitSyncHub.Zwift.JsonSerializerContexts;
using FitSyncHub.Zwift.Protobuf;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Zwift.HttpClients;

public partial class ZwiftHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ZwiftHttpClient> _logger;

    public ZwiftHttpClient(HttpClient httpClient, ILogger<ZwiftHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PlayerProfile> GetProfileMe(CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/profiles/me");
        request.Headers.Add("Accept-Encoding", "gzip, deflate");
        request.Headers.Add("Accept", "application/x-protobuf-lite");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return PlayerProfile.Parser.ParseFrom(stream);
    }

    public async Task<List<int>> GetAchievements(CancellationToken cancellationToken)
    {
        const string Url = "achievement/loadPlayerAchievements";

        var response = await _httpClient.GetAsync(Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var achievements = Achievements.Parser.ParseFrom(stream);

        return [.. achievements.Achievements_.Select(x => x.Id)];
    }

    public async Task<ZwiftGameInfoResponse> GetGameInfo(CancellationToken cancellationToken)
    {
        const string Url = "api/game_info";

        var response = await _httpClient.GetAsync(Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content,
            ZwiftGameInfoGenerationContext.Default.ZwiftGameInfoResponse)!;
    }
}
