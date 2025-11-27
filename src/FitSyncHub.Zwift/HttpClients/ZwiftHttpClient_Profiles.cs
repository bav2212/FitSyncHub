using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Profiles;
using FitSyncHub.Zwift.JsonSerializerContexts;
using FitSyncHub.Zwift.Protobuf;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

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

    public async Task<ZwiftPlayerProfileResponse> GetProfileDetailed(
        long riderId,
        CancellationToken cancellationToken)
    {
        var url = $"api/profiles/{riderId}";

        var response = await _httpClientJson.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(content,
            ZwiftProfilesGenerationContext.Default.ZwiftPlayerProfileResponse)!;
    }

    public async Task<PlayerProfiles> GetProfiles(
        IReadOnlyCollection<long> riderIds,
        CancellationToken cancellationToken)
    {
        var url = QueryHelpers.AddQueryString("api/profiles", new Dictionary<string, StringValues>
        {
            {"id", riderIds.Select(riderId => riderId.ToString()).ToArray() }
        });

        var response = await _httpClientProto.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return PlayerProfiles.Parser.ParseFrom(stream);
    }
}
