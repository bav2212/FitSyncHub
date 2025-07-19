using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FitSyncHub.Common.Extensions;
using FitSyncHub.Common.Services;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;
using FitSyncHub.IntervalsICU.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using nietras.SeparatedValues;

namespace FitSyncHub.Functions.Functions;

public class LactateSyncHttpTriggerFunction
{
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly string _intervalsIcuAthleteId;
    private readonly IDistributedCacheService _distributedCacheService;
    private readonly ILogger<LactateSyncHttpTriggerFunction> _logger;
    private readonly JsonTypeInfo<DateTime> _dateTimeJsonTypeInfo;

    public LactateSyncHttpTriggerFunction(
        IntervalsIcuHttpClient intervalsIcuHttpClient,
         IOptions<IntervalsIcuOptions> intervalsIcuOptions,
        IDistributedCacheService distributedCacheService,
        ILogger<LactateSyncHttpTriggerFunction> logger)
    {
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _intervalsIcuAthleteId = intervalsIcuOptions.Value.AthleteId;
        _distributedCacheService = distributedCacheService;
        _logger = logger;

        _dateTimeJsonTypeInfo = JsonTypeInfo.CreateJsonTypeInfo<DateTime>(JsonSerializerOptions.Default);
    }

    [Function(nameof(LactateSyncHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "lactate-sync")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        if (req.ContentType != "text/csv")
        {
            _logger.LogError("Invalid content type: {ContentType}", req.ContentType);
            return new BadRequestObjectResult("Invalid content type, should be csv file");
        }

        const string CacheKey = "lactate-last-synced-data";

        var lastSyncDate = await _distributedCacheService.GetValueAsync(
            CacheKey,
            _dateTimeJsonTypeInfo,
            cancellationToken);

        if (lastSyncDate == default)
        {
            return new BadRequestObjectResult("No last sync date found in cache, stop to avoid mess");
        }

        var lactateResults = await ParseLactateResults(req, cancellationToken);

        var upcomingLactateResultsData = lactateResults.Where(x => x.Time > lastSyncDate)
            .ToList();

        if (upcomingLactateResultsData.Count == 0)
        {
            _logger.LogInformation("No new lactate data to sync, last sync date: {LastSyncDate}", lastSyncDate);
            return new OkObjectResult("No new lactate data to sync");
        }

        if (upcomingLactateResultsData.Count == 0)
        {
            return new OkObjectResult("No new lactate data to sync");
        }

        var oldestDate = upcomingLactateResultsData.Min(x => x.Time).Date;
        var newestDate = upcomingLactateResultsData.Max(x => x.Time).Date.AddDays(1).AddSeconds(-1);

        var intervalsIcuActivities = await FetchActivities(oldestDate, newestDate, cancellationToken);
        var mappedLactateResultsToActivities = MapActivitiesWithLactateResults(upcomingLactateResultsData, intervalsIcuActivities);

        await UpdateIntervalsIcuActivitiesWithLactateValues(
            mappedLactateResultsToActivities,
            cancellationToken);

        var lastSyncedLactateResultTime = mappedLactateResultsToActivities.SelectMany(x => x.Value).Max(x => x.Time);

        await _distributedCacheService.SetValueAsync(
            CacheKey,
            lastSyncedLactateResultTime,
            _dateTimeJsonTypeInfo,
            cancellationToken
           );

        return new OkObjectResult("Success");
    }

    private async Task UpdateIntervalsIcuActivitiesWithLactateValues(Dictionary<ActivityResponse, List<LactateResult>> mappedLactateResultsToActivities, CancellationToken cancellationToken)
    {
        foreach (var (activity, lactateResults) in mappedLactateResultsToActivities)
        {
            if (activity.Lactate.HasValue)
            {
                _logger.LogWarning("Activity {ActivityId} already has lactate values, skipping", activity.Id);
                continue;
            }

            var orderedLactateResults = lactateResults.OrderBy(x => x.Time).ToList();

            var activityUpdateRequest = new ActivityUpdateRequest
            {
                Lactate = orderedLactateResults.Select(x => x.Value).Last(),
            };

            await _intervalsIcuHttpClient.UpdateActivity(activity.Id, activityUpdateRequest, cancellationToken);

            var lactateNotes = orderedLactateResults
                .ConvertAll(x =>
                {
                    var minutesAfterWorkoutStarted = (int)(x.Time - activity.StartDateLocal).TotalMinutes;

                    return $"{x.Value} mmol/L after {minutesAfterWorkoutStarted} minutes (at {x.Time:HH:mm})";
                })
                .Aggregate(new StringBuilder(),
                    (sb, note) => sb.AppendLine(note),
                    sb => sb.ToString());

            var addMessageRequestModel = new AddMessageRequest
            {
                Content = lactateNotes,
            };

            await _intervalsIcuHttpClient.AddMessage(activity.Id, addMessageRequestModel, cancellationToken);
        }
    }

    private static Dictionary<ActivityResponse, List<LactateResult>> MapActivitiesWithLactateResults(List<LactateResult> upcomingData, List<ActivityResponse> intervalsIcuActivities)
    {
        Dictionary<ActivityResponse, List<LactateResult>> result = [];

        var intervalsIcuActivitiesGroupedByDate = intervalsIcuActivities.GroupBy(x => x.StartDate.Date)
           .ToDictionary(x => x.Key, x => x.OrderBy(x => x.StartDateLocal).ToList());

        var lactateResultsByDate = upcomingData
            .GroupBy(x => x.Time.Date)
            .ToDictionary(x => x.Key, x => x.ToHashSet());

        foreach (var (date, lactateResults) in lactateResultsByDate)
        {
            if (!intervalsIcuActivitiesGroupedByDate.TryGetValue(date, out var intervalsIcuActivitiesByDate))
            {
                throw new InvalidOperationException($"No activities found for date {date:yyyy-MM-dd} in IntervalsICU data.");
            }

            foreach (var lactateResult in lactateResults)
            {
                var lactateResultActivity = intervalsIcuActivities
                    .SingleOrDefault(x => lactateResult.Time >= x.StartDateLocal && lactateResult.Time <= x.EndTimeLocal);

                if (lactateResultActivity == null)
                {
                    // If lactate result is not found in any activity, try to find it in the next 10 minutes of the activity
                    lactateResultActivity = intervalsIcuActivities
                        .SingleOrDefault(x => lactateResult.Time >= x.StartDateLocal
                            && lactateResult.Time <= x.EndTimeLocal.AddMinutes(10));

                    if (lactateResultActivity == null)
                    {
                        throw new InvalidDataException("Could not map all lactate results to activities");
                    }
                }

                if (result.TryGetValue(lactateResultActivity, out var lactateResultsInActivity))
                {
                    lactateResultsInActivity.Add(lactateResult);
                }
                else
                {
                    result.Add(lactateResultActivity, [lactateResult]);
                }
            }
        }

        return result;
    }

    private async Task<List<ActivityResponse>> FetchActivities(DateTime oldestDate, DateTime newestDate, CancellationToken cancellation)
    {
        const int Limit = 30;

        List<ActivityResponse> activities = [];
        for (; ; )
        {
            var activitiesPortionWithStravaActivities = await _intervalsIcuHttpClient
               .ListActivities(_intervalsIcuAthleteId, oldestDate, newestDate, Limit, cancellation);

            var nextPageExists = activitiesPortionWithStravaActivities.Count == Limit;

            var portion = activitiesPortionWithStravaActivities.WhereNotNull().ToList();
            activities.AddRange(portion);

            newestDate = portion.Min(x => x.StartDate.Date);
            if (!nextPageExists)
            {
                break;
            }
        }

        return [.. activities.DistinctBy(x => x.Id)];
    }

    private static async Task<List<LactateResult>> ParseLactateResults(HttpRequest req, CancellationToken cancellationToken)
    {
        List<LactateResult> result = [];

        using var reader = await Sep.Reader().FromAsync(req.Body, cancellationToken);

        var header = reader.Header;
        await foreach (var readRow in reader)           // Read one row at a time
        {
            var indicator = readRow["indicator"].ToString();
            if (indicator != "Lactate")
            {
                continue;
            }

            var time = readRow["time"].Parse<DateTime>();
            var value = double.Parse(readRow["value"].ToString().Replace("mmol/L", ""));

            result.Add(new() { Time = time, Value = value });
        }

        return result;
    }

    private record LactateResult
    {
        public required DateTime Time { get; init; }
        public required double Value { get; init; }
    }
}
