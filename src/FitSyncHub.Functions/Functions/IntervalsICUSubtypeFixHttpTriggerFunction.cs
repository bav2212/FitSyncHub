using FitSyncHub.Common.Extensions;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Common;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using DateTime = System.DateTime;

namespace FitSyncHub.Functions.Functions;

[Obsolete("Run when intervals.icu fix issue with subtypes and remove after it")]
public class IntervalsICUSubtypeFixHttpTriggerFunction
{
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly ILogger<IntervalsICUSubtypeFixHttpTriggerFunction> _logger;

    public IntervalsICUSubtypeFixHttpTriggerFunction(
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        ILogger<IntervalsICUSubtypeFixHttpTriggerFunction> logger)
    {
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _logger = logger;
    }

    [Function(nameof(IntervalsICUSubtypeFixHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "intervals-icu-subtype-fix")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        const int Limit = 100;
        // do not change, just need some old date for api. Limit will handle paging
        var oldest = DateTime.MinValue;

        var newest = DateTime.Now;
        List<ActivityResponse?> activitiesPortion;

        do
        {
            activitiesPortion = [.. await _intervalsIcuHttpClient.ListActivities(
                new ListActivitiesQueryParams(oldest, newest) { Limit = Limit }, cancellationToken)];

            var activities = activitiesPortion
                // skip strava activities
                .WhereNotNull()
                .Where(x => x.Type.Contains("Ride"))
                .ToList();

            await Implementation(activities, cancellationToken);

            // to be sure we don't miss any activities, we move the window by 1 day
            newest = activities.Min(x => x.StartDateLocal).Date.AddDays(1);
        }
        while (activitiesPortion.Count == Limit);

        return new OkObjectResult("Success");
    }

    private async Task Implementation(List<ActivityResponse> activities, CancellationToken cancellationToken)
    {
        var activityGroups = activities
            .GroupBy(x => x.StartDate.Date).ToDictionary(x => x.Key, x => x.ToList());

        foreach (var (date, activitiesForDate) in activityGroups)
        {
            HashSet<DateTime> knownDateToSkipCauseMuptipleRaces = [
                // 4 tiny races
                new DateTime(2025, 8, 23),
                // twoo races
                new DateTime(2024, 12, 22)
            ];

            if (knownDateToSkipCauseMuptipleRaces.Contains(date))
            {
                // skip known bad data
                continue;
            }

            if (!(activitiesForDate.SingleOrDefault(x => x.SubType == ActivitySubType.Race) is { } raceActivity))
            {
                continue;
            }

            var warmupActivities = activitiesForDate
                .Where(x => x.StartDateLocal < raceActivity.StartDateLocal
                    && x != raceActivity
                    && x.SubType != ActivitySubType.Warmup)
                .ToList();
            await UpdateActivitiesSubtype(warmupActivities, ActivitySubType.Warmup, cancellationToken);

            var cooldownActivities = activitiesForDate
                .Where(x => x.StartDateLocal > raceActivity.StartDateLocal
                    && x != raceActivity
                    && x.SubType != ActivitySubType.Cooldown)
                .ToList();
            await UpdateActivitiesSubtype(cooldownActivities, ActivitySubType.Cooldown, cancellationToken);
        }
    }

    private async Task UpdateActivitiesSubtype(
        List<ActivityResponse> activities, ActivitySubType subType, CancellationToken cancellationToken)
    {
        if (activities.Count == 0)
        {
            return;
        }

        foreach (var activity in activities)
        {
            await _intervalsIcuHttpClient.UpdateActivity(activity.Id, new ActivityUpdateRequest
            {
                SubType = subType
            }, cancellationToken: cancellationToken);
        }
    }
}
