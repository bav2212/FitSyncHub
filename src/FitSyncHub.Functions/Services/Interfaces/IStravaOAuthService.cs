using FitSyncHub.Functions.HttpClients.Models.Responses;

namespace FitSyncHub.Functions.Services.Interfaces;

public interface IStravaOAuthService
{
    Task<TokenResponseModel> RequestToken(long athleteId, CancellationToken cancellationToken);
}
