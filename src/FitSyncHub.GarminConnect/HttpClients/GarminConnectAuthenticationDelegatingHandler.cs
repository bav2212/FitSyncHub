using FitSyncHub.GarminConnect.Auth;
using FitSyncHub.GarminConnect.Auth.Abstractions;
using FitSyncHub.GarminConnect.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;

namespace FitSyncHub.GarminConnect.HttpClients;

public class GarminConnectAuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly IGarminAuthenticationService _garminAuthenticationService;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<GarminConnectAuthenticationDelegatingHandler> _logger;

    private readonly string _cacheKey = "GarminOAuth2Token";
    private readonly int _retryCount = 3;
    private readonly int _initialWaitDurationMilliseconds = 200;

    public GarminConnectAuthenticationDelegatingHandler(
        IGarminAuthenticationService garminAuthenticationService,
        IMemoryCache memoryCache,
        ILogger<GarminConnectAuthenticationDelegatingHandler> logger)
    {
        _garminAuthenticationService = garminAuthenticationService;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var authorizationEnsuringPolicy = Policy
            .Handle<GarminConnectAuthenticationException>()
            .WaitAndRetryAsync(
                retryCount: _retryCount,
                retryAttempt => TimeSpan.FromMilliseconds(_initialWaitDurationMilliseconds * Math.Pow(2, retryAttempt - 1)),
                (response, timespan, retryCount, context) =>
                {
                    RemoveCache();
                    _logger.LogWarning("Error while authentication: {Error}, retry {RetryCount} after {Timespan}", response.Message, retryCount, timespan);
                });

        var authenticationResult =
            await authorizationEnsuringPolicy.ExecuteAsync(() => Authenticate(cancellationToken));

        request.Headers.Add("cookie", authenticationResult.Cookie);
        request.Headers.Add("authorization", $"Bearer {authenticationResult.OAuthToken2.AccessToken}");
        request.Headers.Add("di-backend", "connectapi.garmin.com");

        return await base.SendAsync(request, cancellationToken);
    }

    private Task<AuthenticationResult> Authenticate(CancellationToken cancellationToken)
    {
        return _memoryCache.GetOrCreateAsync(_cacheKey, async entry =>
        {
            return await _garminAuthenticationService.RefreshGarminAuthenticationAsync(cancellationToken);
        })!;
    }

    private void RemoveCache()
    {
        _memoryCache.Remove(_cacheKey);
    }
}
