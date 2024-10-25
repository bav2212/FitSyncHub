using System.Net;
using FitSyncHub.Functions.HttpClients.Models.Responses.Activities;

namespace FitSyncHub.Functions.HttpClients.Interfaces;

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
