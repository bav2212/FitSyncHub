using System.Net.Http.Headers;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models;
using FitSyncHub.Strava.Models.Responses;
using FitSyncHub.Strava.Options;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Strava.DelegatingHandlers;

public class StravaAuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly IStravaOAuthHttpClient _stravaAuthHttpClient;
    private readonly IStravaOAuthTokenStore _stravaOAuthTokenStore;
    private readonly long _athleteId;

    public StravaAuthenticationDelegatingHandler(
        IStravaOAuthHttpClient stravaAuthHttpClient,
        IStravaOAuthTokenStore stravaOAuthTokenStore,
        IOptions<StravaOptions> options)
    {
        _stravaAuthHttpClient = stravaAuthHttpClient;
        _stravaOAuthTokenStore = stravaOAuthTokenStore;
        _athleteId = options.Value.AthleteId;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await RequestToken(_athleteId, cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        return await base.SendAsync(request, cancellationToken);
    }

    public async Task<TokenResponseModel> RequestToken(long athleteId, CancellationToken cancellationToken)
    {
        var oauthToken = await _stravaOAuthTokenStore.Get(athleteId, cancellationToken)
            ?? throw new NotImplementedException();

        if (DateTimeOffset.FromUnixTimeSeconds(oauthToken.ExpiresAt) >= DateTimeOffset.UtcNow)
        {
            return new(oauthToken.AccessToken);
        }

        return await RefreshToken(oauthToken, cancellationToken);
    }

    private async Task<TokenResponseModel> RefreshToken(StravaOAuthTokenModel oauthToken, CancellationToken cancellationToken)
    {
        var refreshTokenResponse = await _stravaAuthHttpClient
           .RefreshTokenAsync(oauthToken.RefreshToken, cancellationToken);

        oauthToken.AccessToken = refreshTokenResponse.AccessToken;
        oauthToken.RefreshToken = refreshTokenResponse.RefreshToken;
        oauthToken.ExpiresIn = refreshTokenResponse.ExpiresIn;
        oauthToken.ExpiresAt = refreshTokenResponse.ExpiresAt;
        oauthToken.TokenType = refreshTokenResponse.TokenType;

        await _stravaOAuthTokenStore.Update(oauthToken, cancellationToken);

        return new(oauthToken.AccessToken);
    }
}
