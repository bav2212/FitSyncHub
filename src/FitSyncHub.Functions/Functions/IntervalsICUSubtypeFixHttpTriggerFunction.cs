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

#if DEBUG
    [Function(nameof(IntervalsICUSubtypeFixHttpTriggerFunction))]
#endif
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "intervals-icu-subtype-fix")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _ = req;

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
                .Where(x => x.IsRide)
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
        foreach (var consecutiveActivities in GroupConsecutiveActivities(activities))
        {
            List<ActivityResponse> warmupActivities = [];
            List<ActivityResponse> cooldownActivities = [];
            // very rare, but can have more than one
            List<ActivityResponse> raceActivities = [];

            foreach (var activity in consecutiveActivities)
            {
                if (activity.SubType == ActivitySubType.Race)
                {
                    raceActivities.Add(activity);
                    continue;
                }

                var listToPutActivityIn = raceActivities.Count == 0 ? warmupActivities : cooldownActivities;
                listToPutActivityIn.Add(activity);
            }

            if (raceActivities.Count == 0)
            {
                // should not update anything. No race = no warmup/cooldown
                continue;
            }

            await UpdateActivitiesSubtypeIfNeed(warmupActivities, ActivitySubType.Warmup, cancellationToken);
            await UpdateActivitiesSubtypeIfNeed(cooldownActivities, ActivitySubType.Cooldown, cancellationToken);
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

    private async Task UpdateActivitiesSubtypeIfNeed(
        List<ActivityResponse> activities, ActivitySubType subType, CancellationToken cancellationToken)
    {
        if (activities.Count == 0)
        {
            return;
        }

        foreach (var activity in activities)
        {
            if (activity.SubType == subType)
            {
                continue;
            }

            await _intervalsIcuHttpClient.UpdateActivity(activity.Id, new ActivityUpdateRequest
            {
                SubType = subType
            }, cancellationToken);
        }
    }
}
