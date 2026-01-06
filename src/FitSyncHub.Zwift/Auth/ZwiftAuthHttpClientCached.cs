using System.IdentityModel.Tokens.Jwt;
using FitSyncHub.Common.Services;
using FitSyncHub.Zwift.Auth.Abstractions;
using FitSyncHub.Zwift.JsonSerializerContexts;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Zwift.Auth;

internal sealed class ZwiftAuthHttpClientCached : IZwiftAuthenticator, IZwiftAuthCacheInvalidator
{
    private readonly IDistributedCacheService _distributedCacheService;
    private readonly IZwiftAuthenticator _zwiftAuthenticator;
    private readonly IZwiftTokenRefresher _zwiftTokenRefresher;
    private readonly ILogger<ZwiftAuthHttpClientCached> _logger;

    public ZwiftAuthHttpClientCached(
        IDistributedCacheService distributedCacheService,
        IZwiftAuthenticator zwiftAuthenticator,
        IZwiftTokenRefresher zwiftTokenRefresher,
        ILogger<ZwiftAuthHttpClientCached> logger)
    {
        _distributedCacheService = distributedCacheService;
        _zwiftAuthenticator = zwiftAuthenticator;
        _zwiftTokenRefresher = zwiftTokenRefresher;
        _logger = logger;
    }

    public async Task<ZwiftAuthTokenModel> Authenticate(CancellationToken cancellationToken)
    {
        var cachedResult = await _distributedCacheService.GetValueAsync(
            Common.Constants.CacheKeys.ZwiftAuthTokenModel,
            ZwiftAuthGenerationContext.Default.ZwiftAuthTokenModel,
            cancellationToken);
        if (cachedResult != null && IsTokenValid(cachedResult.AccessToken))
        {
            _logger.LogInformation("Using cached authentication result");
            return cachedResult;
        }

        ZwiftAuthTokenModel authenticationResult;
        if (cachedResult is not null && IsTokenValid(cachedResult.RefreshToken))
        {
            authenticationResult = await _zwiftTokenRefresher.RefreshToken(cachedResult, cancellationToken);
        }
        else
        {
            authenticationResult = await _zwiftAuthenticator.Authenticate(cancellationToken);
        }

        await _distributedCacheService.SetValueAsync(
            Common.Constants.CacheKeys.ZwiftAuthTokenModel,
            authenticationResult,
            ZwiftAuthGenerationContext.Default.ZwiftAuthTokenModel,
            cancellationToken);
        _logger.LogInformation("Cached authentication result");
        return authenticationResult;
    }

    private static bool IsTokenValid(string jwtToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(jwtToken);

        var expiryDate = jwtSecurityToken.ValidTo;
        return expiryDate > DateTime.UtcNow;
    }

    public async Task Invalidate(CancellationToken cancellationToken)
    {
        await _distributedCacheService.RemoveAsync(
            Common.Constants.CacheKeys.ZwiftAuthTokenModel,
            cancellationToken);
    }
}
