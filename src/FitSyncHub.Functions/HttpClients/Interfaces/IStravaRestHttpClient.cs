using FitSyncHub.Functions.HttpClients.Models.BrowserSession;
using FitSyncHub.Functions.HttpClients.Models.Responses.Activities;
using FitSyncHub.Functions.HttpClients.Models.Responses.Athletes;

namespace FitSyncHub.Functions.HttpClients.Interfaces;

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
