using FitSyncHub.Strava.Models.BrowserSession;
using FitSyncHub.Strava.Models.Responses.Activities;
using FitSyncHub.Strava.Models.Responses.Athletes;

namespace FitSyncHub.Strava.Abstractions;

public interface IStravaRestHttpClient
{
    Task<DetailedAthleteResponse> UpdateAthlete(
        float weight,
        CancellationToken cancellationToken);

    Task<List<SummaryActivityModelResponse>> GetActivities(
        long before,
        long after,
        int page,
        int perPage,
        CancellationToken cancellationToken);

    Task<ActivityModelResponse> GetActivity(
        long activityId,
        CancellationToken cancellationToken);

    Task<List<SummaryGearResponse>> GetBikes(CancellationToken cancellationToken);

    Task<ActivityModelResponse> UpdateActivity(
        long activityId,
        UpdatableActivityRequest model,
        CancellationToken cancellationToken);
}
