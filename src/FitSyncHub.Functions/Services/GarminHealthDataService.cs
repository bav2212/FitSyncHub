using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.GarminConnect.Models.Responses;
using FitSyncHub.IntervalsICU;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.Strava.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Services;
public class GarminHealthDataService
{
    private const string StravaLastSetWeightKey = "strava_last_set_weight";
    private const string GarminLastWeightResponseKey = "garmin_last_weight_response";

    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly IStravaRestHttpClient _stravaRestHttpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<GarminHealthDataService> _logger;

    public GarminHealthDataService(
        GarminConnectHttpClient garminConnectHttpClient,
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        IStravaRestHttpClient stravaRestHttpClient,
        IMemoryCache memoryCache,
        ILogger<GarminHealthDataService> logger)
    {
        _garminConnectHttpClient = garminConnectHttpClient;
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _stravaRestHttpClient = stravaRestHttpClient;
        _memoryCache = memoryCache;
        _logger = logger;
    }
    public async Task Sync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        var garminWeightResponse = await _garminConnectHttpClient.GetWeightDayView(today, cancellationToken);

        await UpdateIntervalsIcuWellness(garminWeightResponse, today, cancellationToken);
        await UpdateStravaWeight(garminWeightResponse, cancellationToken);
    }

    private async Task UpdateStravaWeight(
        GarminWeightResponse garminWeightResponse,
        CancellationToken cancellationToken)
    {
        if (!garminWeightResponse.TotalAverage.Weight.HasValue)
        {
            return;
        }

        var weightInKgs = garminWeightResponse.TotalAverage.Weight.Value / 1000;
        if (_memoryCache.TryGetValue(StravaLastSetWeightKey, out float lastSetWeight)
            && lastSetWeight == weightInKgs)
        {
            _logger.LogInformation("Skip strava weight update cause weight didn't change");
            return;
        }

        _logger.LogInformation("Updating weight: {Value}", weightInKgs);
        await _stravaRestHttpClient.UpdateAthlete(weightInKgs, cancellationToken);
        _memoryCache.Set(StravaLastSetWeightKey, weightInKgs);
    }

    private async Task UpdateIntervalsIcuWellness(GarminWeightResponse garminWeightResponse,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        // should be ok for now, cause I didn't stop functions often
        // maybe will store data in database in future
        if (_memoryCache.TryGetValue(GarminLastWeightResponseKey, out GarminWeightResponse? garminLastWeightResonse)
            // check that it works
            && new GarminWeightResponseComparer().Equals(garminLastWeightResonse, garminWeightResponse))
        {
            _logger.LogInformation("Skip intervals.icu wellness data update, cause nothing has changed in Garmin");
            return;
        }

        var intervalsIcuWellness = await _intervalsIcuHttpClient
            .GetWellness(Constants.AthleteId, today, cancellationToken);

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

        _memoryCache.Set(GarminLastWeightResponseKey, garminWeightResponse);
    }
}
