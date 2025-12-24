using FitSyncHub.Functions.Mappers;
using FitSyncHub.Functions.Repositories;
using FitSyncHub.Strava;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models.Responses.Activities;

namespace FitSyncHub.Functions.Services;

public sealed class StravaSummaryActivityService
{
    private readonly IStravaHttpClient _stravaHttpClient;
    private readonly SummaryActivityRepository _summaryActivityRepository;

    public StravaSummaryActivityService(
        IStravaHttpClient stravaHttpClient,
        SummaryActivityRepository summaryActivityRepository)
    {
        _stravaHttpClient = stravaHttpClient;
        _summaryActivityRepository = summaryActivityRepository;
    }

    public async Task<int> StoreSummaryActivities(
        long before,
        long after,
        CancellationToken cancellationToken)
    {
        var activities = await GetSummaryActivities(before, after, cancellationToken);

        var mapper = new SummaryActivityMapper();

        foreach (var activity in activities)
        {
            var dataModel = mapper.SummaryActivityResponseToDataModel(activity);
            _ = await _summaryActivityRepository.UpsertItemAsync(dataModel, cancellationToken);
        }

        return activities.Count;
    }

    public async Task StoreSummaryActivity(
       long activityId,
       CancellationToken cancellationToken)
    {
        var activity = await _stravaHttpClient.GetActivity(activityId, cancellationToken);
        var mapper = new SummaryActivityMapper();

        var dataModel = mapper.ActivityResponseToSummaryDataModel(activity);
        _ = await _summaryActivityRepository.UpsertItemAsync(dataModel, cancellationToken);
    }

    public async Task DeleteSummaryActivity(
       long activityId,
       CancellationToken cancellationToken)
    {
        var existingActivitySummary = await _summaryActivityRepository
            .Read(x => x.Id == activityId.ToString(), cancellationToken);
        if (existingActivitySummary is null)
        {
            return;
        }

        _ = await _summaryActivityRepository
            .DeleteItemAsync(existingActivitySummary, cancellationToken);
    }

    private async Task<List<SummaryActivityModelResponse>> GetSummaryActivities(
        long before,
        long after,
        CancellationToken cancellationToken)
    {
        var result = new List<SummaryActivityModelResponse>();

        const int PerPage = Constants.Api.AthleteActivitiesPerPage;
        var hasValuesToIterate = true;

        for (var page = Constants.Api.AthleteActivitiesFirstPage; hasValuesToIterate; page++)
        {
            var portion = await _stravaHttpClient.GetActivities(before, after, page, PerPage, cancellationToken);
            result.AddRange(portion);

            hasValuesToIterate = portion.Count == PerPage;
        }

        return result;
    }
}
