using FitSyncHub.Common.Services;
using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.GarminConnect.JsonSerializerContexts;
using FitSyncHub.GarminConnect.Models.Responses;
using FitSyncHub.Strava.Abstractions;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Services;

public sealed class GarminHealthDataService
{
    private const string GarminLastWeightResponseKey = "garmin_last_weight_response";

    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly IStravaHttpClient _stravaHttpClient;
    private readonly IDistributedCacheService _distributedCacheService;
    private readonly ILogger<GarminHealthDataService> _logger;

    public GarminHealthDataService(
        GarminConnectHttpClient garminConnectHttpClient,
        IStravaHttpClient stravaHttpClient,
        IDistributedCacheService distributedCacheService,
        ILogger<GarminHealthDataService> logger)
    {
        _garminConnectHttpClient = garminConnectHttpClient;
        _stravaHttpClient = stravaHttpClient;
        _distributedCacheService = distributedCacheService;
        _logger = logger;
    }
    public async Task Sync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        var garminWeightResponse = await _garminConnectHttpClient.GetWeightDayView(today, cancellationToken);
        var previousGarminWeightResponse = await _distributedCacheService.GetValueAsync(
            GarminLastWeightResponseKey,
            GarminConnectWeightSerializerContext.Default.GarminWeightResponse,
            cancellationToken);
        if (previousGarminWeightResponse != null
            && new GarminWeightResponseComparer().Equals(previousGarminWeightResponse, garminWeightResponse))
        {
            _logger.LogInformation("Skip wellness data update, cause nothing has changed in Garmin");
            return;
        }

        if (garminWeightResponse.DateWeightList.Length != 0)
        {
            await UpdateStravaWeight(garminWeightResponse, previousGarminWeightResponse, cancellationToken);
        }

        await _distributedCacheService.SetValueAsync(
            GarminLastWeightResponseKey,
            garminWeightResponse,
            GarminConnectWeightSerializerContext.Default.GarminWeightResponse,
            cancellationToken);
    }

    private async Task UpdateStravaWeight(
        GarminWeightResponse garminWeightResponse,
        GarminWeightResponse? previousGarminWeightResponse,
        CancellationToken cancellationToken)
    {
        if (previousGarminWeightResponse is { }
            && previousGarminWeightResponse.TotalAverage.Weight == garminWeightResponse.TotalAverage.Weight)
        {
            _logger.LogInformation("Skip strava weight update cause weight didn't change");
            return;
        }

        var lastWeightMeasurement = garminWeightResponse.DateWeightList
            .OrderBy(x => x.Date)
            .Last();
        if (lastWeightMeasurement.SourceType != "INDEX_SCALE")
        {
            _logger.LogInformation("Skip strava weight update cause data not from Garmin index scale");
            return;
        }

        var weightInKgs = lastWeightMeasurement.Weight / 1000;

        _logger.LogInformation("Updating weight: {Value}", weightInKgs);
        await _stravaHttpClient.UpdateAthlete(weightInKgs, cancellationToken);
    }
}
