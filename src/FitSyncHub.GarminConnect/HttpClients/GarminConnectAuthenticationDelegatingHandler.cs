using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using FitSyncHub.GarminConnect.Auth.Abstractions;

namespace FitSyncHub.GarminConnect.HttpClients;

public class GarminConnectAuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly IGarminAuthProvider _garminAuthProvider;
    private readonly IGarminTokenRefresher _garminTokenRefresher;
    private readonly IGarminAuthCacheInvalidator _garminAuthCacheInvalidator;

    public GarminConnectAuthenticationDelegatingHandler(
        IGarminAuthProvider garminAuthProvider,
        IGarminTokenRefresher garminTokenRefresher,
        IGarminAuthCacheInvalidator garminAuthCacheInvalidator)
    {
        _garminAuthProvider = garminAuthProvider;
        _garminTokenRefresher = garminTokenRefresher;
        _garminAuthCacheInvalidator = garminAuthCacheInvalidator;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await GetToken(cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        // If the token is invalid, invalidate the cache and retry
        await _garminAuthCacheInvalidator.Invalidate(cancellationToken);
        throw new GarminNotLoggedInException();
    }

    private async Task<string> GetToken(CancellationToken ct)
    {
        var authResult = await _garminAuthProvider.GetAuthResult(ct) ?? throw new GarminNotLoggedInException();

        if (!TokenExpiresSoon(authResult.DiToken))
        {
            return authResult.DiToken;
        }

        authResult = await _garminTokenRefresher.Refresh(authResult, ct);
        return authResult.DiToken;
    }

    private static bool TokenExpiresSoon(string jwtToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(jwtToken);

        var expiryDate = jwtSecurityToken.ValidTo;
        return DateTime.UtcNow > expiryDate.AddMinutes(-5);
    }
}
