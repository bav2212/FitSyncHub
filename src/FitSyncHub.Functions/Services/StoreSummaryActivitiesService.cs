using FitSyncHub.Functions.Mappers;
using FitSyncHub.Functions.Repositories;
using FitSyncHub.Strava;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models.Responses.Activities;

namespace FitSyncHub.Functions.Services;

public class SummaryActivityService
{
    private readonly IStravaRestHttpClient _stravaRestHttpClient;
    private readonly SummaryActivityRepository _summaryActivityRepository;

    public SummaryActivityService(
        IStravaRestHttpClient stravaRestHttpClient,
        SummaryActivityRepository summaryActivityRepository)
    {
        _stravaRestHttpClient = stravaRestHttpClient;
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
            var response = await _summaryActivityRepository.UpsertItemAsync(dataModel, cancellationToken: cancellationToken);
            _ = response;
        }

        return activities.Count;
    }

    public async Task StoreSummaryActivity(
       long activityId,
       CancellationToken cancellationToken)
    {
        var activity = await _stravaRestHttpClient.GetActivity(activityId, cancellationToken);
        var mapper = new SummaryActivityMapper();

        var dataModel = mapper.ActivityResponseToSummaryDataModel(activity);
        var response = await _summaryActivityRepository.UpsertItemAsync(dataModel, cancellationToken: cancellationToken);
        _ = response;
    }

    public async Task DeleteSummaryActivity(
       long activityId,
       CancellationToken cancellationToken)
    {
        var existingActivitySummary = await _summaryActivityRepository
            .Read(x => x.id == activityId.ToString(), cancellationToken);
        if (existingActivitySummary is null)
        {
            return;
        }

        var response = await _summaryActivityRepository
            .DeleteItemAsync(existingActivitySummary, cancellationToken: cancellationToken);
        _ = response;
    }

    private async Task<List<SummaryActivityModelResponse>> GetSummaryActivities(
        long before,
        long after,
        CancellationToken cancellationToken)
    {
        var result = new List<SummaryActivityModelResponse>();

        const int PerPage = Constants.StravaRestApi.AthleteActivitiesPerPage;
        var hasValuesToIterate = true;

        for (var page = Constants.StravaRestApi.AthleteActivitiesFirstPage; hasValuesToIterate; page++)
        {
            var portion = await _stravaRestHttpClient.GetActivities(before, after, page, PerPage, cancellationToken);
            result.AddRange(portion);

            hasValuesToIterate = portion.Count == PerPage;
        }

        return result;
    }
}
