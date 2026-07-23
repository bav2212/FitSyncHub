using FitSyncHub.Common.Services;
using FitSyncHub.GarminConnect.Auth.Abstractions;
using FitSyncHub.GarminConnect.Auth.HttpClients;
using FitSyncHub.GarminConnect.Auth.Models;
using FitSyncHub.GarminConnect.JsonSerializerContexts;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.GarminConnect.Auth;

internal class GarminAuthService :
    IGarminTokenSetter,
    IGarminTokenRefresher,
    IGarminTokenProvider,
    IGarminTokenInvalidator
{
    private readonly IDistributedCacheService _distributedCacheService;
    private readonly GarminDiHttpClient _diHttpClient;
    private readonly ILogger<GarminAuthService> _logger;

    public GarminAuthService(
        IDistributedCacheService distributedCacheService,
        GarminDiHttpClient diHttpClient,
        ILogger<GarminAuthService> logger)
    {
        _distributedCacheService = distributedCacheService;
        _diHttpClient = diHttpClient;
        _logger = logger;
    }

    public Task SetTokenModel(GarminDiTokenModel tokenModel, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tokenModel.DiToken)
            || string.IsNullOrWhiteSpace(tokenModel.DiRefreshToken)
            || string.IsNullOrWhiteSpace(tokenModel.DiClientId))
        {
            throw new ArgumentException("tokenModel is not valid", nameof(tokenModel));
        }

        return CacheDiTokenModel(tokenModel, cancellationToken);
    }

    public async Task<GarminDiTokenModel> Refresh(GarminDiTokenModel tokenModel, CancellationToken cancellationToken)
    {
#pragma warning disable CA1873 // Avoid potentially expensive logging
        _logger.LogInformation("Starting token refresh with token: {Token}, {ClientId}",
                tokenModel.DiToken, tokenModel.DiClientId);
#pragma warning restore CA1873 // Avoid potentially expensive logging
        var diTokenModel = await _diHttpClient.Refresh(tokenModel, cancellationToken);
        _logger.LogInformation("Refreshed di token");

        await CacheDiTokenModel(diTokenModel, cancellationToken);
        return diTokenModel;
    }

    public async Task<GarminDiTokenModel?> GetTokenModel(CancellationToken cancellationToken)
    {
        return await _distributedCacheService.GetValueAsync(
           Common.Constants.CacheKeys.GarminDiTokenModel,
           GarminAuthSerializerContext.Default.GarminDiTokenModel,
           cancellationToken);
    }

    public async Task Invalidate(CancellationToken cancellationToken)
    {
        await _distributedCacheService.RemoveAsync(
            Common.Constants.CacheKeys.GarminDiTokenModel,
            cancellationToken);
    }

    private async Task CacheDiTokenModel(GarminDiTokenModel diTokenModel, CancellationToken cancellationToken)
    {
        await _distributedCacheService.SetValueAsync(
            Common.Constants.CacheKeys.GarminDiTokenModel,
            diTokenModel,
            GarminAuthSerializerContext.Default.GarminDiTokenModel,
            cancellationToken);
    }
}
