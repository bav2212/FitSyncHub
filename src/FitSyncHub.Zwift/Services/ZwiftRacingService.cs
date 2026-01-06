using FitSyncHub.Common.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace FitSyncHub.Zwift.Services;

public class ZwiftRacingService
{
    private readonly IDistributedCacheService _distributedCacheService;

    public ZwiftRacingService(IDistributedCacheService distributedCacheService)
    {
        _distributedCacheService = distributedCacheService;
    }

    public async Task SetCookie(string cookie, CancellationToken cancellationToken)
    {
        await _distributedCacheService.SetStringAsync(
             Common.Constants.CacheKeys.ZwiftRacingAuthCookie,
             cookie,
             cancellationToken);
    }
}
