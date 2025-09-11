using System.Net.Http.Headers;
using FitSyncHub.Xert.Abstractions;

namespace FitSyncHub.Xert.DelegatingHandlers;
public class XertAuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly IXertAuthService _xertAuthService;

    public XertAuthenticationDelegatingHandler(
        IXertAuthService xertAuthService)
    {
        _xertAuthService = xertAuthService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _xertAuthService.RequestToken(cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
