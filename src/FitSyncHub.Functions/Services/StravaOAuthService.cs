using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Functions.Repositories;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models.Responses;

namespace FitSyncHub.Functions.Services;

public class StravaOAuthService : IStravaOAuthService
{
    private readonly IStravaOAuthHttpClient _stravaAuthHttpClient;
    private readonly PersistedGrantRepository _persistedGrantRepository;

    public StravaOAuthService(
        IStravaOAuthHttpClient stravaAuthHttpClient,
        PersistedGrantRepository persistedGrantRepository)
    {
        _stravaAuthHttpClient = stravaAuthHttpClient;
        _persistedGrantRepository = persistedGrantRepository;
    }

    public async Task<TokenResponseModel> RequestToken(long athleteId, CancellationToken cancellationToken)
    {
        var persistedGrant = await GetPersistedGrant(athleteId, cancellationToken)
            ?? throw new NotImplementedException();

        if (DateTimeOffset.FromUnixTimeSeconds(persistedGrant.ExpiresAt) >= DateTimeOffset.UtcNow)
        {
            return new(persistedGrant.AccessToken);
        }

        return await RefreshToken(persistedGrant, cancellationToken);
    }

    private async Task<TokenResponseModel> RefreshToken(PersistedGrant persistedGrant, CancellationToken cancellationToken)
    {
        var refreshTokenResponse = await _stravaAuthHttpClient
            .RefreshTokenAsync(persistedGrant.RefreshToken, cancellationToken);

        persistedGrant.AccessToken = refreshTokenResponse.AccessToken;
        persistedGrant.RefreshToken = refreshTokenResponse.RefreshToken;
        persistedGrant.ExpiresIn = refreshTokenResponse.ExpiresIn;
        persistedGrant.ExpiresAt = refreshTokenResponse.ExpiresAt;
        persistedGrant.TokenType = refreshTokenResponse.TokenType;

        await _persistedGrantRepository.UpsertItemAsync(persistedGrant, cancellationToken: cancellationToken);

        return new(persistedGrant.AccessToken);
    }

    private async Task<PersistedGrant?> GetPersistedGrant(long athleteId, CancellationToken cancellationToken)
    {
        return await _persistedGrantRepository.Read(x => x.AthleteId == athleteId, cancellationToken);
    }
}
