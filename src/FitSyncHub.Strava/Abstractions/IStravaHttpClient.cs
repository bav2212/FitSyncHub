using FitSyncHub.Common.Models;
using FitSyncHub.Strava.HttpClients.Models.Requests;
using FitSyncHub.Strava.HttpClients.Models.Responses;
using FitSyncHub.Strava.HttpClients.Models.Responses.Activities;
using FitSyncHub.Strava.HttpClients.Models.Responses.Athletes;
using FitSyncHub.Strava.Models.Requests;

namespace FitSyncHub.Strava.Abstractions;

public interface IStravaHttpClient
{
    Task<DetailedAthleteResponse> UpdateAthlete(
        UpdateAthleteRequest model,
        CancellationToken cancellationToken);

    Task<List<SummaryActivityModelResponse>> GetActivities(
        GetActivitiesRequest model,
        CancellationToken cancellationToken);

    Task<ActivityModelResponse> GetActivity(
        long activityId,
        CancellationToken cancellationToken);

    Task<List<SummaryGearResponse>> GetBikes(
        CancellationToken cancellationToken);

    Task<ActivityModelResponse> UpdateActivity(
        long activityId,
        UpdatableActivityRequest model,
        CancellationToken cancellationToken);

    Task<UploadActivityResponse> UploadStart(
        FileModel file,
        StartUploadActivityRequest model,
        CancellationToken cancellationToken);

    Task<UploadActivityResponse> GetUpload(
       long uploadId,
       CancellationToken cancellationToken);
}
