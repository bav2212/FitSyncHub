using FitSyncHub.Common.Services;
using FitSyncHub.Zwift.HttpClients.Abstractions;
using FitSyncHub.Zwift.JsonSerializerContexts;
using FitSyncHub.Zwift.Models.FRR;

namespace FitSyncHub.Zwift.Cached;

internal class FlammeRougeRacingHttpClientCached : IFlammeRougeRacingHttpClient
{
    private readonly IFlammeRougeRacingHttpClient _flammeRougeRacingHttpClient;
    private readonly IDistributedCacheService _distributedCacheService;

    public FlammeRougeRacingHttpClientCached(
        IFlammeRougeRacingHttpClient flammeRougeRacingHttpClient,
        IDistributedCacheService distributedCacheService)
    {
        _flammeRougeRacingHttpClient = flammeRougeRacingHttpClient;
        _distributedCacheService = distributedCacheService;
    }

    public async Task<List<long>> GetTourRegisteredRiders(FlammeRougeRacingCategory flammeRougeRacingCategory, CancellationToken cancellationToken)
    {
        const string CacheKeyPrefix = Common.Constants.CacheKeys.FlammeRougeRacingTourRegisteredRiderIdsPrefix;
        var cacheKey = $"{CacheKeyPrefix}-category-{flammeRougeRacingCategory.ToString().ToLowerInvariant()}";

        var jsonTypeInfo = DistributedCacheGenerationContext.Default.FlammeRougeRacingTourRegisteredRiderIds;

        var cachedResult = await _distributedCacheService.GetValueAsync(cacheKey, jsonTypeInfo, cancellationToken);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var result = await _flammeRougeRacingHttpClient.GetTourRegisteredRiders(flammeRougeRacingCategory, cancellationToken);
        await _distributedCacheService.SetValueAsync(cacheKey, result, jsonTypeInfo, new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        }, cancellationToken);
        return result;
    }

    public Task<List<FlammeRougeRacingEGapResultModel>> GetStageEGap(
        FlammeRougeRacingCategory flammeRougeRacingCategory,
        int stageNumber,
        CancellationToken cancellationToken)
    {
        return _flammeRougeRacingHttpClient.GetStageEGap(flammeRougeRacingCategory, stageNumber, cancellationToken);
    }
}
