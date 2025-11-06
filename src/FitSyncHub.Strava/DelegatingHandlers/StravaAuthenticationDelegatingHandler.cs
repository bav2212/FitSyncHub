using System.Net.Http.Headers;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Options;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Strava.DelegatingHandlers;

public class StravaAuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly IStravaOAuthService _stravaOAuthService;
    private readonly long _athleteId;

    public StravaAuthenticationDelegatingHandler(
        IStravaOAuthService stravaOAuthService,
        IOptions<StravaOptions> options)
    {
        _stravaOAuthService = stravaOAuthService;
        _athleteId = options.Value.AthleteId;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await _stravaOAuthService.RequestToken(_athleteId, cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}
