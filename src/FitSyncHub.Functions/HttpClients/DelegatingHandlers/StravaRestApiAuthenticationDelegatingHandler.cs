using System.Net.Http.Headers;
using FitSyncHub.Functions.Services.Interfaces;

namespace FitSyncHub.Functions.HttpClients.DelegatingHandlers;
internal class StravaRestApiAuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly AthleteContext _athleteContext;
    private readonly IStravaOAuthService _stravaOAuthService;

    public StravaRestApiAuthenticationDelegatingHandler(
        AthleteContext athleteContext,
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
