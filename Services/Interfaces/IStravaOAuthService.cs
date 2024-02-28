using StravaWebhooksAzureFunctions.HttpClients.Models.Responses;
using System.Threading.Tasks;

namespace StravaWebhooksAzureFunctions.Services.Interfaces;

public interface IStravaOAuthService
{
    Task<TokenResponseModel> RequestToken(long athleteId);
}
