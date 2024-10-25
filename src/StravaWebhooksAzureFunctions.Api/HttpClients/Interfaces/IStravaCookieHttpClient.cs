using System.Net;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activities;

namespace StravaWebhooksAzureFunctions.HttpClients.Interfaces;

public interface IStravaCookieHttpClient
{
    Task<HttpResponseMessage> UpdateActivityVisibilityToOnlyMe(
         ActivityModelResponse activity,
         CookieContainer cookies,
         string authenticityToken,
         Func<DateTime, string> privateNoteFormatter,
         CancellationToken cancellationToken);

    Task<HttpResponseMessage?> CorrectElevationIfNeeded(
         long activityId,
         CookieContainer cookies,
         string authenticityToken,
         CancellationToken cancellationToken);
}
