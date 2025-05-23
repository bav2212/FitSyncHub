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
    IGarminTokenExchanger,
    IGarminAuthProvider,
    IGarminAuthCacheInvalidator
{
    private readonly string _authenticationCacheKey = "garmin-oauth2-token";
    private readonly string _mfaClientStateCacheKey = "garmin-oauth2-mfa-client-state";

    private readonly IDistributedCacheService _distributedCacheService;
    private readonly GarminSsoHttpClient _ssoHttpClient;
    private readonly GarminOAuthHttpClient _oAuthHttpClient;
    private readonly IGarminConsumerCredentialsProvider _consumerCredentialsProvider;
    private readonly ILogger<GarminAuthService> _logger;

    public GarminAuthService(
        IDistributedCacheService distributedCacheService,
        GarminSsoHttpClient ssoHttpClient,
        GarminOAuthHttpClient oAuthHttpClient,
        IGarminConsumerCredentialsProvider consumerCredentialsProvider,
        ILogger<GarminAuthService> logger)
    {
        _distributedCacheService = distributedCacheService;
        _ssoHttpClient = ssoHttpClient;
        _oAuthHttpClient = oAuthHttpClient;
        _consumerCredentialsProvider = consumerCredentialsProvider;
        _logger = logger;
    }

    public async Task<GarminLoginResult> Login(CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ssoHttpClient.Login(cancellationToken);
            var auth = await CompleteLogin(ticket, cancellationToken);

            await CacheAuthResult(auth, cancellationToken);

            return new GarminLoginResult { AuthenticationResult = auth };
        }
        catch (GarminConnectNeedsMfaException ex)
        {
            await _distributedCacheService.SetValueAsync(
                _mfaClientStateCacheKey,
                ex.ClientState,
                GarminConnectOAuthSerializerContext.Default.GarminNeedsMfaClientState,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                }, cancellationToken);

            return new GarminLoginResult { MfaRequired = true };
        }
    }

    public async Task<GarminAuthenticationResult> ResumeLogin(string mfaCode, CancellationToken cancellationToken)
    {
        var cachedMfaClientState = await _distributedCacheService.GetValueAsync(
            _mfaClientStateCacheKey,
            GarminConnectOAuthSerializerContext.Default.GarminNeedsMfaClientState,
            cancellationToken)
            ?? throw new InvalidDataException("no mfa client state cached");

        var ticket = await _ssoHttpClient.ResumeLogin(mfaCode, cachedMfaClientState, cancellationToken);
        var authResult = await CompleteLogin(ticket, cancellationToken);

        await Invalidate(cancellationToken);
        await CacheAuthResult(authResult, cancellationToken);

        return authResult;
    }

    public async Task<GarminAuthenticationResult> ExchangeToken(GarminOAuth1Token oAuth1Token, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting token exchange with token: {Token}, secret: {Secret}",
            oAuth1Token.Token, oAuth1Token.TokenSecret);

        var consumerCredentials = await _consumerCredentialsProvider.GetConsumerCredentials(cancellationToken);
        _logger.LogInformation("Consumer Key for exchange: {ConsumerKey}", consumerCredentials.ConsumerKey);

        var token = await _oAuthHttpClient.Exchange(oAuth1Token, consumerCredentials, cancellationToken);
        _logger.LogInformation("Exchanged OAuth2 token: {AccessToken}", token.AccessToken);

        var authResult = new GarminAuthenticationResult
        {
            OAuthToken1 = oAuth1Token,
            OAuthToken2 = token,
        };

        await CacheAuthResult(authResult, cancellationToken);

        return authResult;
    }

    public async Task<GarminAuthenticationResult?> GetAuthResult(CancellationToken cancellationToken)
    {
        return await _distributedCacheService.GetValueAsync(
           _authenticationCacheKey,
           GarminConnectOAuthSerializerContext.Default.GarminAuthenticationResult,
           cancellationToken);
    }

    public async Task Invalidate(CancellationToken cancellationToken)
    {
        await _distributedCacheService.RemoveAsync(_authenticationCacheKey, cancellationToken);
        await _distributedCacheService.RemoveAsync(_mfaClientStateCacheKey, cancellationToken);
    }

    private async Task CacheAuthResult(GarminAuthenticationResult auth, CancellationToken cancellationToken)
    {
        await _distributedCacheService.SetValueAsync(
            _authenticationCacheKey,
            auth,
            GarminConnectOAuthSerializerContext.Default.GarminAuthenticationResult,
            cancellationToken);
    }

    private async Task<GarminAuthenticationResult> CompleteLogin(string ticket, CancellationToken cancellationToken)
    {
        var consumerCredentials = await _consumerCredentialsProvider.GetConsumerCredentials(cancellationToken);

        var oauth1 = await _oAuthHttpClient.GetOAuth1Token(ticket, consumerCredentials, cancellationToken);
        _logger.LogInformation("OAuth1 token: {Token}, secret: {Secret}", oauth1.Token, oauth1.TokenSecret);

        var oauth2 = await _oAuthHttpClient.Exchange(oauth1, consumerCredentials, cancellationToken);
        _logger.LogInformation("OAuth2 token: {AccessToken}", oauth2.AccessToken);

        _logger.LogInformation("Authentication process completed.");
        return new GarminAuthenticationResult
        {
            OAuthToken1 = oauth1,
            OAuthToken2 = oauth2,
        };
    }
}
