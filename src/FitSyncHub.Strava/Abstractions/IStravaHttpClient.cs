using FitSyncHub.Common.Models;
using FitSyncHub.Strava.Models.Requests;
using FitSyncHub.Strava.Models.Responses;
using FitSyncHub.Strava.Models.Responses.Activities;
using FitSyncHub.Strava.Models.Responses.Athletes;

namespace FitSyncHub.Strava.Abstractions;

public interface IStravaHttpClient
{
    Task<DetailedAthleteResponse> UpdateAthlete(
        float weight,
        CancellationToken cancellationToken);

    Task<List<SummaryActivityModelResponse>> GetActivities(
        long before,
        long after,
        int page,
        int perPage,
        CancellationToken cancellationToken);

    Task<ActivityModelResponse> GetActivity(
        long activityId,
        CancellationToken cancellationToken);

    Task<List<SummaryGearResponse>> GetBikes(CancellationToken cancellationToken);

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
