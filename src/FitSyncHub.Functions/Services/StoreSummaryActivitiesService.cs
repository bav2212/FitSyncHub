using FitSyncHub.Functions.HttpClients.Interfaces;
using FitSyncHub.Functions.HttpClients.Models.Responses.Activities;
using FitSyncHub.Functions.Mappers;
using FitSyncHub.Functions.Repositories;

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
        long athleteId,
        long before,
        long after,
        CancellationToken cancellationToken)
    {
        var activities = await GetSummaryActivities(athleteId, before, after, cancellationToken);

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
       long athleteId,
       long activityId,
       CancellationToken cancellationToken)
    {
        var activity = await _stravaRestHttpClient.GetActivity(activityId, athleteId, cancellationToken);
        var mapper = new SummaryActivityMapper();

        var dataModel = mapper.ActivityResponseToSummaryDataModel(activity);
        var response = await _summaryActivityRepository.UpsertItemAsync(dataModel, cancellationToken: cancellationToken);
        _ = response;
    }

    public async Task DeleteSummaryActivity(
       long athleteId,
       long activityId,
       CancellationToken cancellationToken)
    {
        var existingActivitySummary = await _summaryActivityRepository
            .Read(x => x.id == activityId.ToString(), cancellationToken);
        if (existingActivitySummary is null)
        {
            return;
        }

        var response = await _summaryActivityRepository.DeleteItemAsync(existingActivitySummary, cancellationToken: cancellationToken);
        _ = response;
    }

    private async Task<List<SummaryActivityModelResponse>> GetSummaryActivities(
        long athleteId,
        long before,
        long after,
        CancellationToken cancellationToken)
    {
        var result = new List<SummaryActivityModelResponse>();

        var perPage = Constants.StravaRestApi.AthleteActivitiesPerPage;
        var hasValuesToIterate = true;

        for (var page = Constants.StravaRestApi.AthleteActivitiesFirstPage; hasValuesToIterate; page++)
        {
            var portion = await _stravaRestHttpClient.GetActivities(athleteId, before, after, page, perPage, cancellationToken);
            result.AddRange(portion);

            hasValuesToIterate = portion.Count == perPage;
        }

        return result;
    }
}
