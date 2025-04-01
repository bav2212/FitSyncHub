using FitSyncHub.Common.Extensions;
using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.GarminConnect.Models.Responses;
using FitSyncHub.IntervalsICU;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.Strava.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Services;
public class GarminHealthDataService
{
    private const string GarminLastWeightResponseKey = "garmin_last_weight_response";

    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly IStravaRestHttpClient _stravaRestHttpClient;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<GarminHealthDataService> _logger;

    public GarminHealthDataService(
        GarminConnectHttpClient garminConnectHttpClient,
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        IStravaRestHttpClient stravaRestHttpClient,
        IDistributedCache distributedCache,
        ILogger<GarminHealthDataService> logger)
    {
        _garminConnectHttpClient = garminConnectHttpClient;
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _stravaRestHttpClient = stravaRestHttpClient;
        _distributedCache = distributedCache;
        _logger = logger;
    }
    public async Task Sync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        var garminWeightResponse = await _garminConnectHttpClient.GetWeightDayView(today, cancellationToken);
        var previousGarminWeightResponse = await _distributedCache.GetFromJsonAsync<GarminWeightResponse>(GarminLastWeightResponseKey, cancellationToken);
        if (previousGarminWeightResponse != null && new GarminWeightResponseComparer().Equals(previousGarminWeightResponse, garminWeightResponse))
        {
            _logger.LogInformation("Skip wellness data update, cause nothing has changed in Garmin");
            return;
        }

        await UpdateIntervalsIcuWellness(garminWeightResponse, previousGarminWeightResponse, today, cancellationToken);
        await UpdateStravaWeight(garminWeightResponse, previousGarminWeightResponse, cancellationToken);

        await _distributedCache.SetAsJsonAsync(GarminLastWeightResponseKey, garminWeightResponse, cancellationToken: cancellationToken);
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

        var lastWeightMeasurement = garminWeightResponse.DateWeightList[^1];
        if (lastWeightMeasurement.SourceType != "INDEX_SCALE")
        {
            _logger.LogInformation("Skip strava weight update cause data not from Garmin index scale");
            return;
        }

        var weightInKgs = lastWeightMeasurement.Weight / 1000;

        _logger.LogInformation("Updating weight: {Value}", weightInKgs);
        await _stravaRestHttpClient.UpdateAthlete(weightInKgs, cancellationToken);
    }

    private async Task UpdateIntervalsIcuWellness(GarminWeightResponse garminWeightResponse,
        GarminWeightResponse? previousGarminWeightResponse,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var intervalsIcuWellness = await _intervalsIcuHttpClient
            .GetWellness(Constants.AthleteId, today, cancellationToken);

        if (previousGarminWeightResponse is { }
             && previousGarminWeightResponse.TotalAverage.BodyFat == garminWeightResponse.TotalAverage.BodyFat)
        {
            _logger.LogInformation("Skip wellness data update, cause nothing has changed in Garmin");
            return;
        }

        if (garminWeightResponse.TotalAverage.BodyFat != intervalsIcuWellness.BodyFat)
        {
            var wellnessRequest = new WellnessRequest
            {
                Id = intervalsIcuWellness.Id!,
                BodyFat = garminWeightResponse.TotalAverage.BodyFat
            };

            _logger.LogInformation("Updating Intervals.icu wellness: {Value}", wellnessRequest);
            var response = await _intervalsIcuHttpClient.UpdateWellness(Constants.AthleteId, wellnessRequest, cancellationToken);
            _logger.LogInformation("Updated Intervals.icu wellness. Response: {Value}", response);
        }
    }
}
