using Microsoft.Azure.Cosmos;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activities;
using StravaWebhooksAzureFunctions.Mappers;

namespace StravaWebhooksAzureFunctions.Services;
public class StoreActivitiesService
{
    private readonly IStravaRestHttpClient _stravaRestHttpClient;
    private readonly Container _activitiesContainer;

    public StoreActivitiesService(
        IStravaRestHttpClient stravaRestHttpClient,
        CosmosClient cosmosClient)
    {
        _stravaRestHttpClient = stravaRestHttpClient;
        _activitiesContainer = cosmosClient.GetDatabase("strava").GetContainer("SummaryActivity");
    }

    public async Task StoreActivities(
        long athleteId,
        long before,
        long after,
        CancellationToken cancellationToken)
    {
        var activities = await GetActivities(athleteId, before, after, cancellationToken);

        var mapper = new SummaryActivityMapper();

        foreach (var activity in activities)
        {
            var dataModel = mapper.SummaryActivityResponseToDataModel(activity);
            var response = await _activitiesContainer.UpsertItemAsync(dataModel, cancellationToken: cancellationToken);
            _ = response;
        }
    }

    private async Task<List<SummaryActivityModelResponse>> GetActivities(
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
