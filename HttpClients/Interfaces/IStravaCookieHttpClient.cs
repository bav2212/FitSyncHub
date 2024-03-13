using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activity;
using System.Net;

namespace StravaWebhooksAzureFunctions.HttpClients.Interfaces;

public interface IStravaCookieHttpClient
{
    Task<HttpResponseMessage> UpdateActivityVisibilityToOnlyMe(
         ActivityModelResponse activity,
         CookieContainer cookies,
         string authenticityToken,
         Func<string> privateNoteFormatter,
         CancellationToken cancellationToken);
}
