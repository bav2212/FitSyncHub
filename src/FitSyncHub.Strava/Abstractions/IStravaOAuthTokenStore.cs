using FitSyncHub.Strava.Models;

namespace FitSyncHub.Strava.Abstractions;

public interface IStravaOAuthTokenStore
{
    Task<StravaOAuthTokenModel> Get(long athleteId, CancellationToken cancellationToken);
    Task<StravaOAuthTokenModel> Create(StravaOAuthTokenModel stravaOAuthToken, CancellationToken cancellationToken);
    Task<StravaOAuthTokenModel> Update(StravaOAuthTokenModel stravaOAuthToken, CancellationToken cancellationToken);
}
