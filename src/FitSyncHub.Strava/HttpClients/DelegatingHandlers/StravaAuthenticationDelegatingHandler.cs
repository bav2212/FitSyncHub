using System.Net.Http.Headers;
using FitSyncHub.Strava.Services;

namespace FitSyncHub.Strava.HttpClients.DelegatingHandlers;

public class StravaAuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly StravaOAuthTokenService _stravaOAuthTokenService;

    public StravaAuthenticationDelegatingHandler(StravaOAuthTokenService stravaOAuthTokenService)
    {
        _stravaOAuthTokenService = stravaOAuthTokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await _stravaOAuthTokenService.RequestToken(cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}
