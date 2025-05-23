using FitSyncHub.Common.Services;
using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.GarminConnect.JsonSerializerContexts;
using FitSyncHub.GarminConnect.Models.Responses;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.Options;
using FitSyncHub.Strava.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Functions.Services;
public class GarminHealthDataService
{
    private const string GarminLastWeightResponseKey = "garmin_last_weight_response";

    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly IStravaHttpClient _stravaHttpClient;
    private readonly IDistributedCacheService _distributedCacheService;
    private readonly string _intervalsIcuAthleteId;
    private readonly ILogger<GarminHealthDataService> _logger;

    public GarminHealthDataService(
        GarminConnectHttpClient garminConnectHttpClient,
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        IStravaHttpClient stravaHttpClient,
        IDistributedCacheService distributedCacheService,
        IOptions<IntervalsIcuOptions> intervalsIcuOptions,
        ILogger<GarminHealthDataService> logger)
    {
        _garminConnectHttpClient = garminConnectHttpClient;
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _stravaHttpClient = stravaHttpClient;
        _distributedCacheService = distributedCacheService;
        _intervalsIcuAthleteId = intervalsIcuOptions.Value.AthleteId;
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
            await UpdateIntervalsIcuWellness(garminWeightResponse, previousGarminWeightResponse, today, cancellationToken);
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

    private async Task UpdateIntervalsIcuWellness(GarminWeightResponse garminWeightResponse,
        GarminWeightResponse? previousGarminWeightResponse,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var intervalsIcuWellness = await _intervalsIcuHttpClient
            .GetWellness(_intervalsIcuAthleteId, today, cancellationToken);

        if (previousGarminWeightResponse is { }
             && previousGarminWeightResponse.TotalAverage.BodyFat == garminWeightResponse.TotalAverage.BodyFat)
        {
            _logger.LogInformation("Skip wellness data update, cause nothing has changed in Garmin");
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

        if (!lastWeightMeasurement.BodyFat.HasValue)
        {
            _logger.LogDebug("Skip wellness data update, cause body fat is not available in Garmin");
            return;
        }

        if (lastWeightMeasurement.BodyFat.Value != intervalsIcuWellness.BodyFat)
        {
            var wellnessRequest = new WellnessRequest
            {
                Id = intervalsIcuWellness.Id!,
                BodyFat = lastWeightMeasurement.BodyFat.Value,
            };

            _logger.LogInformation("Updating Intervals.icu wellness: {Value}", wellnessRequest);
            var response = await _intervalsIcuHttpClient.UpdateWellness(_intervalsIcuAthleteId, wellnessRequest, cancellationToken);
            _logger.LogInformation("Updated Intervals.icu wellness. Response: {Value}", response);
        }
    }
}
