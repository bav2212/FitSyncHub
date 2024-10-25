using FitSyncHub.Functions.HttpClients.Models.Responses;

namespace FitSyncHub.Functions.HttpClients.Interfaces;

public interface IStravaOAuthHttpClient
{
    Task<ExchangeTokenResponse> ExchangeTokenAsync(string code, CancellationToken cancellationToken);
    Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}
