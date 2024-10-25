using StravaWebhooksAzureFunctions.HttpClients.Models.Responses;

namespace StravaWebhooksAzureFunctions.HttpClients.Interfaces;

public interface IStravaOAuthHttpClient
{
    Task<ExchangeTokenResponse> ExchangeTokenAsync(string code, CancellationToken cancellationToken);
    Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}
