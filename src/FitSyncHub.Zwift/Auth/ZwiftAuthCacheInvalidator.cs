using FitSyncHub.Common.Services;
using FitSyncHub.Zwift.Auth.Abstractions;

namespace FitSyncHub.Zwift.Auth;

internal sealed class ZwiftAuthCacheInvalidator : IZwiftAuthCacheInvalidator
{
    private readonly IDistributedCacheService _distributedCacheService;

    public ZwiftAuthCacheInvalidator(IDistributedCacheService distributedCacheService)
    {
        _distributedCacheService = distributedCacheService;
    }

    public Task Invalidate(CancellationToken cancellationToken)
    {
        return _distributedCacheService.RemoveAsync(
            Common.Constants.CacheKeys.ZwiftAuthTokenModel,
            cancellationToken);
    }
}
