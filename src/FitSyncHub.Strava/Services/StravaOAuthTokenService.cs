using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models;
using FitSyncHub.Strava.Models.Responses;
using FitSyncHub.Strava.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Strava.Services;

public class StravaOAuthTokenService
{
    private readonly HashSet<string> _expectedScope = [
        "read",
        "activity:write",
        "activity:read",
        "activity:read_all",
        "profile:write",
        "profile:read_all",
        "read_all"
    ];

    private readonly IStravaOAuthHttpClient _stravaAuthHttpClient;
    private readonly IStravaOAuthTokenStore _stravaOAuthTokenStore;
    private readonly StravaOptions _options;
    private readonly ILogger<StravaOAuthTokenService> _logger;

    public StravaOAuthTokenService(
        IStravaOAuthHttpClient stravaAuthHttpClient,
        IStravaOAuthTokenStore stravaOAuthTokenStore,
        IOptions<StravaOptions> options,
        ILogger<StravaOAuthTokenService> logger)
    {
        _stravaAuthHttpClient = stravaAuthHttpClient;
        _stravaOAuthTokenStore = stravaOAuthTokenStore;
        _options = options.Value;
        _logger = logger;
    }

    public async Task ExchangeTokenAsync(string scope, string code, CancellationToken cancellationToken)
    {
        if (scope == null || !_expectedScope.SetEquals(scope.Split(',')))
        {
            throw new InvalidDataException("Invalid scope");
        }

        var exchangeTokenResponse = await _stravaAuthHttpClient.ExchangeTokenAsync(code, cancellationToken);

        _logger.LogInformation("Exchanged token");

        var athleteId = exchangeTokenResponse.Athlete.Id;
        if (athleteId != _options.AthleteId)
        {
            throw new InvalidDataException($"Received athlete id {athleteId} does not match expected athlete id");
        }

        var stravaOAuthTokenModel = new StravaOAuthTokenModel
        {
            TokenType = exchangeTokenResponse.TokenType,
            ExpiresAt = exchangeTokenResponse.ExpiresAt,
            ExpiresIn = exchangeTokenResponse.ExpiresIn,
            RefreshToken = exchangeTokenResponse.RefreshToken,
            AccessToken = exchangeTokenResponse.AccessToken,
            AthleteId = exchangeTokenResponse.Athlete.Id,
            AthleteUserName = exchangeTokenResponse.Athlete.Username,
        };

        await _stravaOAuthTokenStore.Create(stravaOAuthTokenModel, cancellationToken);
    }

    public async Task<TokenResponseModel> RequestToken(CancellationToken cancellationToken)
    {
        var oauthToken = await _stravaOAuthTokenStore.Get(_options.AthleteId, cancellationToken)
            ?? throw new NotImplementedException();

        if (DateTimeOffset.FromUnixTimeSeconds(oauthToken.ExpiresAt) >= DateTimeOffset.UtcNow)
        {
            return new(oauthToken.AccessToken);
        }

        return await RefreshToken(oauthToken, cancellationToken);
    }

    private async Task<TokenResponseModel> RefreshToken(StravaOAuthTokenModel oauthToken, CancellationToken cancellationToken)
    {
        var refreshTokenResponse = await _stravaAuthHttpClient
           .RefreshTokenAsync(oauthToken.RefreshToken, cancellationToken);

        oauthToken.AccessToken = refreshTokenResponse.AccessToken;
        oauthToken.RefreshToken = refreshTokenResponse.RefreshToken;
        oauthToken.ExpiresIn = refreshTokenResponse.ExpiresIn;
        oauthToken.ExpiresAt = refreshTokenResponse.ExpiresAt;
        oauthToken.TokenType = refreshTokenResponse.TokenType;

        await _stravaOAuthTokenStore.Update(oauthToken, cancellationToken);

        return new(oauthToken.AccessToken);
    }
}
