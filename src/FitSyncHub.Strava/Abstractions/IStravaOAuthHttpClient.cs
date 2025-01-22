using FitSyncHub.Strava.Models.Responses;

namespace FitSyncHub.Strava.Abstractions;

public interface IStravaOAuthHttpClient
{
    Task<ExchangeTokenResponse> ExchangeTokenAsync(string code, CancellationToken cancellationToken);
    Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}
