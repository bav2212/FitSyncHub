using StravaWebhooksAzureFunctions.HttpClients.Models.Requests;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activities;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Athletes;

namespace StravaWebhooksAzureFunctions.HttpClients.Interfaces;

public interface IStravaRestHttpClient
{
    Task<DetailedAthleteResponse> UpdateAthlete(
        long athleteId,
        float weight,
        CancellationToken cancellationToken);

    Task<ActivityModelResponse> GetActivity(
        long activityId,
        long athleteId,
        CancellationToken cancellationToken);

    Task<List<SummaryGearResponse>> GetBikes(
        long athleteId,
        CancellationToken cancellationToken);

    Task<ActivityModelResponse> UpdateActivity(
        long activityId,
        long athleteId,
        UpdatableActivityRequest model,
        CancellationToken cancellationToken);
}
