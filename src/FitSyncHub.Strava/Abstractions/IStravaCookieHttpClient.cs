using System.Net;
using FitSyncHub.Strava.Models.Responses.Activities;

namespace FitSyncHub.Strava.Abstractions;

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
