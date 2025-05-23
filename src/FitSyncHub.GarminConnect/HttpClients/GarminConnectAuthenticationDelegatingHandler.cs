using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using FitSyncHub.GarminConnect.Auth.Abstractions;
using FitSyncHub.GarminConnect.Auth.Models;

namespace FitSyncHub.GarminConnect.HttpClients;

public class GarminConnectAuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly IGarminAuthProvider _garminAuthProvider;
    private readonly IGarminTokenExchanger _garminTokenExchanger;
    private readonly IGarminAuthCacheInvalidator _garminAuthCacheInvalidator;

    public GarminConnectAuthenticationDelegatingHandler(
        IGarminAuthProvider garminAuthProvider,
        IGarminTokenExchanger garminTokenExchanger,
        IGarminAuthCacheInvalidator garminAuthCacheInvalidator)
    {
        _garminAuthProvider = garminAuthProvider;
        _garminTokenExchanger = garminTokenExchanger;
        _garminAuthCacheInvalidator = garminAuthCacheInvalidator;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var oauth2Token = await GetToken(cancellationToken);

        request.Headers.Remove("di-backend");

        request.Headers.Authorization = new AuthenticationHeaderValue(oauth2Token.TokenType, oauth2Token.AccessToken);
        request.Headers.Add("di-backend", "connectapi.garmin.com");

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        // If the token is invalid, invalidate the cache and retry
        await _garminAuthCacheInvalidator.Invalidate(cancellationToken);
        throw new GarminNotLoggedInException();
    }

    private async Task<GarminOAuth2Token> GetToken(CancellationToken ct)
    {
        var authResult = await _garminAuthProvider.GetAuthResult(ct) ?? throw new GarminNotLoggedInException();
        var oauth2Token = authResult.OAuthToken2;

        if (IsTokenValid(oauth2Token.AccessToken))
        {
            return oauth2Token;
        }

        authResult = await _garminTokenExchanger.ExchangeToken(authResult.OAuthToken1, ct);
        return authResult.OAuthToken2;
    }

    private static bool IsTokenValid(string jwtToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(jwtToken);

        var expiryDate = jwtSecurityToken.ValidTo;
        return expiryDate > DateTime.UtcNow;
    }
}
