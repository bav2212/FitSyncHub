using System.Net.Http.Headers;
using FitSyncHub.Strava.Abstractions;

namespace FitSyncHub.Strava.DelegatingHandlers;
public class StravaRestApiAuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly StravaAthleteContext _athleteContext;
    private readonly IStravaOAuthService _stravaOAuthService;

    public StravaRestApiAuthenticationDelegatingHandler(
        StravaAthleteContext athleteContext,
        IStravaOAuthService stravaOAuthService)
    {
        _athleteContext = athleteContext;
        _stravaOAuthService = stravaOAuthService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await _stravaOAuthService.RequestToken(_athleteContext.AthleteId, cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}
