using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activities;
using System.Net;

namespace StravaWebhooksAzureFunctions.HttpClients.Interfaces;

public interface IStravaCookieHttpClient
{
    Task<HttpResponseMessage> UpdateActivityVisibilityToOnlyMe(
         ActivityModelResponse activity,
         CookieContainer cookies,
         string authenticityToken,
         Func<DateTime, string> privateNoteFormatter,
         CancellationToken cancellationToken);
}
