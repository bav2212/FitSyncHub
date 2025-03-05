using FitSyncHub.GarminConnect.Models.Auth;
using Garmin.Connect.Auth;
using Garmin.Connect.Auth.External;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;

namespace FitSyncHub.GarminConnect;

public class GarminConnectAuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly IAuthParameters _authParameters;
    private readonly IGarminAuthenticationService _garminAuthenticationService;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<GarminConnectAuthenticationDelegatingHandler> _logger;

    private readonly string _cacheKey = "GarminOAuth2Token";
    private readonly int _retryCount = 3;
    private readonly int _initialWaitDurationMilliseconds = 200;

    public GarminConnectAuthenticationDelegatingHandler(
        IAuthParameters authParameters,
        IGarminAuthenticationService garminAuthenticationService,
        IMemoryCache memoryCache,
        ILogger<GarminConnectAuthenticationDelegatingHandler> logger)
    {
        _authParameters = authParameters;
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

        var oAuth2Token =
            await authorizationEnsuringPolicy.ExecuteAsync(() => Refresh(cancellationToken));

        request.Headers.Add("cookie", _authParameters.Cookies);
        request.Headers.Add("authorization", $"Bearer {oAuth2Token.AccessToken}");
        request.Headers.Add("di-backend", "connectapi.garmin.com");

        return await base.SendAsync(request, cancellationToken);
    }

    private Task<OAuth2Token> Refresh(CancellationToken cancellationToken)
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
