using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activity;

namespace StravaWebhooksAzureFunctions.HttpClients.Interfaces;

public interface IStravaRestHttpClient
{
    Task<ActivityModelResponse> GetActivity(long activityId, long athleteId, CancellationToken cancellationToken);
}
