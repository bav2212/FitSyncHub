using System.Net;
using FitSyncHub.Common.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace FitSyncHub.Zwift.HttpClients.DelegatingHandlers;

public sealed class ZwiftRacingAuthDelegatingHandler : DelegatingHandler
{
    private readonly IDistributedCacheService _distributedCacheService;

    public ZwiftRacingAuthDelegatingHandler(
        IDistributedCacheService distributedCacheService)
    {
        _distributedCacheService = distributedCacheService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var zwiftRacingAuthCookie = await _distributedCacheService
            .GetStringAsync(Common.Constants.CacheKeys.ZwiftRacingAuthCookie, cancellationToken);
        if (string.IsNullOrEmpty(zwiftRacingAuthCookie))
        {
            // Do not attempt the request if we don't have an auth cookie
            throw CreateUnauthorizedAccessException();
        }

        request.Headers.Add("Cookie", zwiftRacingAuthCookie);
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _distributedCacheService.RemoveAsync(
                Common.Constants.CacheKeys.ZwiftRacingAuthCookie,
                cancellationToken);

            throw CreateUnauthorizedAccessException();
        }

        return response;
    }

    private static UnauthorizedAccessException CreateUnauthorizedAccessException()
    {
        return new UnauthorizedAccessException("Zwift Racing authentication cookie is missing.");
    }
}
