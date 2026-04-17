using FitSyncHub.Common.Services;
using FitSyncHub.GarminConnect.Auth.Abstractions;
using FitSyncHub.GarminConnect.Auth.Exceptions;
using FitSyncHub.GarminConnect.Auth.HttpClients;
using FitSyncHub.GarminConnect.Auth.Models;
using FitSyncHub.GarminConnect.JsonSerializerContexts;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.GarminConnect.Auth;

internal class GarminAuthService : IGarminAuthService,
    IGarminTokenRefresher,
    IGarminAuthProvider,
    IGarminAuthCacheInvalidator
{
    private readonly IDistributedCacheService _distributedCacheService;
    private readonly GarminSsoHttpClient _ssoHttpClient;
    private readonly GarminDiHttpClient _diHttpClient;
    private readonly ILogger<GarminAuthService> _logger;

    public GarminAuthService(
        IDistributedCacheService distributedCacheService,
        GarminSsoHttpClient ssoHttpClient,
        GarminDiHttpClient diHttpClient,
        ILogger<GarminAuthService> logger)
    {
        _distributedCacheService = distributedCacheService;
        _ssoHttpClient = ssoHttpClient;
        _diHttpClient = diHttpClient;
        _logger = logger;
    }

    public async Task<GarminLoginResult> Login(CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ssoHttpClient.Login(cancellationToken);
            var auth = await CompleteLogin(ticket, cancellationToken);

            await CacheDiTokenModel(auth, cancellationToken);

            return new GarminLoginResult { DiTokenModel = auth };
        }
        catch (GarminConnectNeedsMfaException ex)
        {
            await _distributedCacheService.SetValueAsync(
                Common.Constants.CacheKeys.GarminMfaClientState,
                ex.ClientState,
                GarminAuthSerializerContext.Default.GarminNeedsMfaClientState,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                }, cancellationToken);

            return new GarminLoginResult { MfaRequired = true };
        }
    }

    public async Task<GarminDiTokenModel> ResumeLogin(string mfaCode, CancellationToken cancellationToken)
    {
        var cachedMfaClientState = await _distributedCacheService.GetValueAsync(
            Common.Constants.CacheKeys.GarminMfaClientState,
            GarminAuthSerializerContext.Default.GarminNeedsMfaClientState,
            cancellationToken)
            ?? throw new InvalidDataException("no mfa client state cached");

        var ticket = await _ssoHttpClient.ResumeLogin(mfaCode, cachedMfaClientState, cancellationToken);
        var authResult = await CompleteLogin(ticket, cancellationToken);

        await Invalidate(cancellationToken);
        await CacheDiTokenModel(authResult, cancellationToken);

        return authResult;
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

    public async Task<GarminDiTokenModel?> GetAuthResult(CancellationToken cancellationToken)
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
        await _distributedCacheService.RemoveAsync(
            Common.Constants.CacheKeys.GarminMfaClientState,
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

    private async Task<GarminDiTokenModel> CompleteLogin(string ticket, CancellationToken cancellationToken)
    {
        //use to implement login https://github.com/cyberjunky/python-garminconnect
        throw new NotImplementedException();
    }
}
