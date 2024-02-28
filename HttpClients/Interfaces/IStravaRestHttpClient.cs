using StravaWebhooksAzureFunctions.HttpClients.Models.Responses;
using System.Threading.Tasks;

namespace StravaWebhooksAzureFunctions.HttpClients.Interfaces;

public interface IStravaRestHttpClient
{
    Task<ActivityModelResponse> GetActivity(long activityId, long athleteId);
}
