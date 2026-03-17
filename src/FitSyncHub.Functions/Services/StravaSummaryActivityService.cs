using FitSyncHub.Functions.Mappers;
using FitSyncHub.Functions.Repositories;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models.Responses.Activities;

namespace FitSyncHub.Functions.Services;

public sealed class StravaSummaryActivityService
{
    private readonly IStravaHttpClient _stravaHttpClient;
    private readonly StravaSummaryActivityRepository _summaryActivityRepository;
    private readonly StravaSummaryActivityMapper _mapper;

    public StravaSummaryActivityService(
        IStravaHttpClient stravaHttpClient,
        StravaSummaryActivityRepository summaryActivityRepository)
    {
        _stravaHttpClient = stravaHttpClient;
        _summaryActivityRepository = summaryActivityRepository;
        _mapper = new StravaSummaryActivityMapper();
    }

    public async Task<int> StoreSummaryActivities(
        long before,
        long after,
        CancellationToken cancellationToken)
    {
        var activities = await GetSummaryActivities(before, after, cancellationToken);

        foreach (var activity in activities)
        {
            var dataModel = _mapper.SummaryActivityResponseToDataModel(activity);
            _ = await _summaryActivityRepository.UpsertItemAsync(dataModel, cancellationToken);
        }

        return activities.Count;
    }

    public async Task StoreSummaryActivity(
       long activityId,
       CancellationToken cancellationToken)
    {
        var activity = await _stravaHttpClient.GetActivity(activityId, cancellationToken);

        var dataModel = _mapper.ActivityResponseToSummaryDataModel(activity);
        _ = await _summaryActivityRepository.UpsertItemAsync(dataModel, cancellationToken);
    }

    public async Task DeleteSummaryActivity(
       long activityId,
       CancellationToken cancellationToken)
    {
        var existingActivitySummary = await _summaryActivityRepository.Read(activityId.ToString(), cancellationToken);
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

        const int PerPage = Strava.Constants.Api.AthleteActivitiesPerPage;
        var hasValuesToIterate = true;

        for (var page = Strava.Constants.Api.AthleteActivitiesFirstPage; hasValuesToIterate; page++)
        {
            var portion = await _stravaHttpClient.GetActivities(before, after, page, PerPage, cancellationToken);
            result.AddRange(portion);

            hasValuesToIterate = portion.Count == PerPage;
        }

        return result;
    }
}
