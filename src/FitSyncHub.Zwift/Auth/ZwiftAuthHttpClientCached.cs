using System.IdentityModel.Tokens.Jwt;
using FitSyncHub.Common.Services;
using FitSyncHub.Zwift.Auth.Abstractions;
using FitSyncHub.Zwift.JsonSerializerContexts;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Zwift.Auth;

internal class ZwiftAuthHttpClientCached : IZwiftAuthenticator, IZwiftAuthCacheInvalidator
{
    private readonly string _authenticationCacheKey = "zwift-auth";

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

    public async Task<ZwiftAuthToken> Authenticate(CancellationToken cancellationToken)
    {
        var cachedResult = await _distributedCacheService.GetValueAsync(
            _authenticationCacheKey,
            ZwiftAuthGenerationContext.Default.ZwiftAuthToken,
            cancellationToken);
        if (cachedResult != null && IsTokenValid(cachedResult.AccessToken))
        {
            _logger.LogInformation("Using cached authentication result");
            return cachedResult;
        }

        ZwiftAuthToken authenticationResult;
        if (cachedResult is not null && IsTokenValid(cachedResult.RefreshToken))
        {
            authenticationResult = await _zwiftTokenRefresher.RefreshToken(cachedResult, cancellationToken);
        }
        else
        {
            authenticationResult = await _zwiftAuthenticator.Authenticate(cancellationToken);
        }

        await _distributedCacheService.SetValueAsync(
            _authenticationCacheKey,
            authenticationResult,
            ZwiftAuthGenerationContext.Default.ZwiftAuthToken,
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
        await _distributedCacheService.RemoveAsync(_authenticationCacheKey, cancellationToken);
    }
}
