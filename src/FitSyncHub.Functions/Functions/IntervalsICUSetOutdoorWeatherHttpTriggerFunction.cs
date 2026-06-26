using FitSyncHub.Common.Abstractions;
using FitSyncHub.Common.Extensions;
using FitSyncHub.Common.Models;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using DateTime = System.DateTime;

namespace FitSyncHub.Functions.Functions;

public class IntervalsICUSetOutdoorWeatherHttpTriggerFunction
{
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly IWeatherService _weatherService;
    private readonly ILogger<IntervalsICUSetOutdoorWeatherHttpTriggerFunction> _logger;

    public IntervalsICUSetOutdoorWeatherHttpTriggerFunction(
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        IWeatherService weatherService,
        ILogger<IntervalsICUSetOutdoorWeatherHttpTriggerFunction> logger)
    {
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _weatherService = weatherService;
        _logger = logger;
    }

#if DEBUG
    [Function(nameof(IntervalsICUSetOutdoorWeatherHttpTriggerFunction))]
#endif
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "intervals-icu-set-outdoor-weather")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        string? fromQueryParameter = req.Query["from"];
        if (!DateOnly.TryParse(fromQueryParameter, out var fromDateOnly))
        {
            return new BadRequestObjectResult("Invalid or missing 'from' query parameter");
        }

        string? toQueryParameter = req.Query["to"];
        if (!DateOnly.TryParse(toQueryParameter, out var toDateOnly))
        {
            toDateOnly = DateOnly.FromDateTime(DateTime.Now);
        }

        string? latitudeQueryParameter = req.Query["lat"];
        if (!double.TryParse(latitudeQueryParameter, out var latitude))
        {
            return new BadRequestObjectResult("Invalid or missing 'lat' query parameter");
        }

        string? longitudeQueryParameter = req.Query["lon"];
        if (!double.TryParse(longitudeQueryParameter, out var longitude))
        {
            return new BadRequestObjectResult("Invalid or missing 'lon' query parameter");
        }

        var coordinate = new Coordinate { Latitude = latitude, Longitude = longitude };

        const int Limit = 100;

        var oldest = fromDateOnly;
        var newest = toDateOnly;

        List<ActivityResponse?> activitiesPortion;

        do
        {
            activitiesPortion = [.. await _intervalsIcuHttpClient.ListActivities(
                new ListActivitiesQueryParams(oldest, newest) { Limit = Limit }, cancellationToken)];

            var activities = activitiesPortion
                // skip strava activities
                .WhereNotNull()
                .Where(x => x.IsRide)
                // do it for virtual rider only
                .Where(x => x.Type == "VirtualRide")
                .ToList();

            await Implementation(activities, coordinate, cancellationToken);

            // to be sure we don't miss any activities, we move the window by 1 day
            newest = DateOnly.FromDateTime(activities.Min(x => x.StartDateLocal).Date.AddDays(1));
        }
        while (activitiesPortion.Count == Limit);

        return new OkObjectResult("Success");
    }

    private async Task Implementation(
        List<ActivityResponse> activities,
        Coordinate coordinate,
        CancellationToken cancellationToken)
    {
        foreach (var consecutiveActivities in GroupConsecutiveActivities(activities))
        {
            if (consecutiveActivities.All(activity => activity.OutsideAvgWeatherTemp.HasValue
                || activity.OutsideMinWeatherTemp.HasValue
                || activity.OutsideMaxWeatherTemp.HasValue)
                )
            {
                continue;
            }

            var startDate = consecutiveActivities.Min(x => x.StartDate);
            var startLocal = consecutiveActivities.Min(x => x.StartDateLocal);

            var localDelta = startLocal - startDate;

            var startDateOffset = new DateTimeOffset(startDate, TimeSpan.Zero);

            var endDate = consecutiveActivities.Max(x => x.EndTimeLocal);
            var endDateOffset = new DateTimeOffset(endDate, TimeSpan.FromHours(localDelta.TotalHours));

            endDateOffset = endDateOffset.UtcDateTime.ToUniversalTime();

            var historicalWeatherData = await _weatherService
                .GetHistoricalWeatherData(coordinate, startDateOffset, endDateOffset, cancellationToken);

            await UpdateIntervalsIcuActivities(consecutiveActivities, historicalWeatherData, cancellationToken);
        }
    }

    private async Task UpdateIntervalsIcuActivities(List<ActivityResponse> consecutiveActivities, List<WeatherModel> historicalWeatherData, CancellationToken cancellationToken)
    {
        var avgHistoricalTemperature = Math.Round(historicalWeatherData.Average(x => x.Temperature), 1);
        var minHistoricalTemperature = historicalWeatherData.Min(x => x.Temperature);
        var maxHistoricalTemperature = historicalWeatherData.Max(x => x.Temperature);

        foreach (var activity in consecutiveActivities)
        {
            await _intervalsIcuHttpClient.UpdateActivity(activity.Id, new ActivityUpdateRequest
            {
                OutsideAvgWeatherTemp = avgHistoricalTemperature,
                OutsideMinWeatherTemp = minHistoricalTemperature,
                OutsideMaxWeatherTemp = maxHistoricalTemperature
            }, cancellationToken);
        }
    }

    private static List<List<ActivityResponse>> GroupConsecutiveActivities(List<ActivityResponse> activities)
    {
        return [.. activities
           .OrderBy(a => a.StartDateLocal)
           .GroupBy(a => a.StartDateLocal.Date)
           .SelectMany(dayGroup =>
           {
               var result = new List<List<ActivityResponse>>();
               List<ActivityResponse>? currentGroup = null;

               foreach (var activity in dayGroup.OrderBy(a => a.StartDateLocal))
               {
                   if (currentGroup == null)
                   {
                       currentGroup = [activity];
                       result.Add(currentGroup);
                       continue;
                   }

                   var lastActivity = currentGroup[^1];
                   var gap = activity.EndTimeLocal - lastActivity.StartDateLocal;

                   if (gap.TotalHours > 2)
                   {
                       currentGroup = [];
                       result.Add(currentGroup);
                   }

                   currentGroup.Add(activity);
               }

               return result;
           })];
    }
}
