using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StravaWebhooksAzureFunctions.HttpClients.Interfaces;

public interface IStravaCookieHttpClient
{
    Task<HttpResponseMessage> UpdateActivityVisibilityToOnlyMe(
         long activityId,
         CookieContainer cookies,
         string authenticityToken,
         CancellationToken cancellationToken);
}
