using System.Net.Http.Headers;
using FitSyncHub.Zwift.Auth.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace FitSyncHub.Zwift.HttpClients.DelegatingHandlers;

public class ZwiftAuthDelegatingHandler : DelegatingHandler
{
    private readonly IZwiftAuthenticator _authClient;
    private readonly ResiliencePipeline<HttpResponseMessage> _authResiliencePipeline;

    public ZwiftAuthDelegatingHandler(
        IZwiftAuthenticator authClient,
        [FromKeyedServices(Constants.ZwiftAuthResiliencePipeline)] ResiliencePipeline<HttpResponseMessage> resiliencePipeline)
    {
        _authClient = authClient;
        _authResiliencePipeline = resiliencePipeline;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return await _authResiliencePipeline.ExecuteAsync(async (ct) =>
        {
            var authResult = await _authClient.Authenticate(ct);

            request.Headers.Authorization = new AuthenticationHeaderValue(authResult.TokenType, authResult.AccessToken);

            return await base.SendAsync(request, ct);
        }, cancellationToken);
    }
}
