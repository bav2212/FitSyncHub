using StravaWebhooksAzureFunctions.HttpClients.Models.Responses;

namespace StravaWebhooksAzureFunctions.Services.Interfaces;

public interface IStravaOAuthService
{
    Task<TokenResponseModel> RequestToken(long athleteId, CancellationToken cancellationToken);
}
