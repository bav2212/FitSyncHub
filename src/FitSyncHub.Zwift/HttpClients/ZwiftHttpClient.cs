using FitSyncHub.Zwift.Protobuf;

namespace FitSyncHub.Zwift.HttpClients;

public partial class ZwiftHttpClient
{
    private readonly HttpClient _httpClient;

    public ZwiftHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<int>> GetAchievements(CancellationToken cancellationToken)
    {
        const string Url = "/achievement/loadPlayerAchievements";

        var response = await _httpClient.GetAsync(Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var achievements = Achievements.Parser.ParseFrom(stream);

        return [.. achievements.Achievements_.Select(x => x.Id)];
    }
}
