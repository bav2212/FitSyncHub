using FitSyncHub.Strava.Models.Responses;

namespace FitSyncHub.Strava.Abstractions;

public interface IStravaOAuthService
{
    Task<TokenResponseModel> RequestToken(long athleteId, CancellationToken cancellationToken);
}
