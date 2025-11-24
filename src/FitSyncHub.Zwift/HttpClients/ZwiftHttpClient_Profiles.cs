using FitSyncHub.Zwift.Protobuf;

namespace FitSyncHub.Zwift.HttpClients;

public sealed partial class ZwiftHttpClient
{
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

    public async Task<PlayerProfile> GetProfile(long riderId, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/profiles/{riderId}");
        request.Headers.Add("Accept-Encoding", "gzip, deflate");
        request.Headers.Add("Accept", "application/x-protobuf-lite");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return PlayerProfile.Parser.ParseFrom(stream);
    }
}
