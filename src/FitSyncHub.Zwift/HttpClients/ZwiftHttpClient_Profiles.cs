using FitSyncHub.Zwift.Protobuf;

namespace FitSyncHub.Zwift.HttpClients;

public sealed partial class ZwiftHttpClient
{
    public async Task<PlayerProfile> GetProfileMe(CancellationToken cancellationToken)
    {
        const string Url = "api/profiles/me";

        var response = await _httpClientProto.GetAsync(Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return PlayerProfile.Parser.ParseFrom(stream);
    }

    public async Task<PlayerProfile> GetProfile(long riderId, CancellationToken cancellationToken)
    {
        var url = $"api/profiles/{riderId}";

        var response = await _httpClientProto.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return PlayerProfile.Parser.ParseFrom(stream);
    }
}
