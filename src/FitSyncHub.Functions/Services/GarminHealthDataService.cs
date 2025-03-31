using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.GarminConnect.Models.Responses;
using FitSyncHub.IntervalsICU;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.Strava.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace FitSyncHub.Functions.Services;
public class GarminHealthDataService
{
    private const string StravaLastSetWeightKey = "strava_last_set_weight";
    private const string GarminLastWeightResponseKey = "garmin_last_weight_response";

    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly IStravaRestHttpClient _stravaRestHttpClient;
    private readonly IMemoryCache _memoryCache;

    public GarminHealthDataService(
        GarminConnectHttpClient garminConnectHttpClient,
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        IStravaRestHttpClient stravaRestHttpClient,
        IMemoryCache memoryCache)
    {
        _garminConnectHttpClient = garminConnectHttpClient;
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _stravaRestHttpClient = stravaRestHttpClient;
        _memoryCache = memoryCache;
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
            return;
        }

        await _stravaRestHttpClient.UpdateAthlete(weightInKgs, cancellationToken);
        _memoryCache.Set(StravaLastSetWeightKey, weightInKgs);
    }

    private async Task UpdateIntervalsIcuWellness(GarminWeightResponse garminWeightResponse,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        // should be ok for now, cause I didn't stop functions often
        // maybe will store data in database in future
        if (_memoryCache.TryGetValue(GarminLastWeightResponseKey, out GarminWeightResponse? garminLastWeightResponsne)
            // check that it works
            && new GarminWeightResponseComparer().Equals(garminLastWeightResponsne, garminWeightResponse))
        {
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

            await _intervalsIcuHttpClient.UpdateWellness(Constants.AthleteId, wellnessRequest, cancellationToken);
        }

        _memoryCache.Set(GarminLastWeightResponseKey, garminWeightResponse);
    }
}
