using Dynastream.Fit;
using FitSyncHub.Common;
using FitSyncHub.Common.Fit;
using FitSyncHub.GarminConnect;
using FitSyncHub.IntervalsICU;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using DateTime = System.DateTime;

namespace FitSyncHub.Functions.Functions;

public class MergeIntervalsICUActivitiesHttpTriggerFunction
{
    private readonly FitFileDecoder _decoder;
    private readonly FitFileEncoder _encoder;
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly ExtendedGarminConnectClient _extendedGarminConnectClient;
    private readonly ILogger<MergeIntervalsICUActivitiesHttpTriggerFunction> _logger;

    public MergeIntervalsICUActivitiesHttpTriggerFunction(
        FitFileDecoder decoder,
        FitFileEncoder encoder,
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        ExtendedGarminConnectClient extendedGarminConnectClient,
        ILogger<MergeIntervalsICUActivitiesHttpTriggerFunction> logger)
    {
        _decoder = decoder;
        _encoder = encoder;
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _extendedGarminConnectClient = extendedGarminConnectClient;
        _logger = logger;
    }

    [Function(nameof(MergeIntervalsICUActivitiesHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "merge-intervals-icu-activities")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        string? countQueryParameter = req.Query["count"];
        string? dateQueryParameter = req.Query["date"];
        string? syncWithGarminQueryParameter = req.Query["syncWithGarmin"];

        if (countQueryParameter is null)
        {
            _logger.LogInformation("wrong request");
            return new BadRequestObjectResult("wrong request");
        }

        if (!int.TryParse(countQueryParameter, out var count))
        {
            _logger.LogInformation("Count has wrong format");
            return new BadRequestObjectResult("Count has wrong format");
        }

        if (count == 0)
        {
            _logger.LogInformation("Count should be more than 0");
            return new BadRequestObjectResult("Count should be more than 0");
        }

        if (count > 10)
        {
            _logger.LogInformation("Can't parse more that 10 activities");
            return new BadRequestObjectResult("Can't parse more that 10 activities");
        }

        if (!DateOnly.TryParse(dateQueryParameter, out var date))
        {
            date = DateOnly.FromDateTime(DateTime.Today);
        }

        var activities = await _intervalsIcuHttpClient.ListActivities(Constants.AthleteId,
            new DateTime(date, TimeOnly.MinValue), new DateTime(date, TimeOnly.MaxValue), 10, cancellationToken) ?? [];
        _logger.LogInformation("Received {ActivitiesCount} activities", activities.Count);

        if (activities.Count != count)
        {
            _logger.LogInformation("Found {ActivitiesCount} todays activities, but specified {ParsedCount} in request", activities.Count, count);
            return new BadRequestObjectResult($"Found {activities.Count} todays activities, but specified {count} in request");
        }

        SyncGarminModel syncWithGarminModel;
        if (activities.Count == 1)
        {
            var activity = activities.Single();

            syncWithGarminModel = new SyncGarminModel
            {
                Name = activity.Name,
                Description = activity.Description,
                Distance = activity.Distance,
                TotalElevationGain = activity.TotalElevationGain
            };
        }
        else
        {
            syncWithGarminModel
                = await UpdateActivitiesWithNewTssAndPrepareGarminSyncModel(activities, cancellationToken);
        }

        if (bool.TryParse(syncWithGarminQueryParameter, out var syncWithGarmin) && syncWithGarmin)
        {
            await SyncWithGarmin(syncWithGarminModel, cancellationToken);
        }

        return new OkObjectResult("Success");
    }

    private async Task<SyncGarminModel> UpdateActivitiesWithNewTssAndPrepareGarminSyncModel(
        IReadOnlyCollection<ActivityResponse> activities,
        CancellationToken cancellationToken)
    {
        EventResponse? linkedPairedEvent = default;
        var activityWithLinkedPairedEvent = activities.Where(x => x.PairedEventId.HasValue).SingleOrDefault();
        if (activityWithLinkedPairedEvent != null && activityWithLinkedPairedEvent.PairedEventId is { } pairedEventId)
        {
            _logger.LogInformation("Get linked paired event");
            linkedPairedEvent = await _intervalsIcuHttpClient.GetEvent(Constants.AthleteId, pairedEventId, cancellationToken);

            _logger.LogInformation("Unlinking paired event {EventId} from activity {ActivityId}", linkedPairedEvent, activityWithLinkedPairedEvent.Id);
            await _intervalsIcuHttpClient.UnlinkPairedWorkout(activityWithLinkedPairedEvent.Id, cancellationToken);
        }

        Dictionary<string, FitMessages> activityFitMessages = [];
        foreach (var activity in activities)
        {
            _logger.LogInformation("Downloading fit file for activity {ActivityId}", activity.Id);
            var memoryStream = await _intervalsIcuHttpClient.DownloadOriginalActivityFitFile(activity.Id, cancellationToken);
            _logger.LogInformation("Downloaded fit file for activity {ActivityId}", activity.Id);

            _logger.LogInformation("Start decoding fit file for activity {ActivityId}", activity.Id);
            var fitMessages = _decoder.Decode(memoryStream);
            _logger.LogInformation("Finished decoding fit file for activity {ActivityId}", activity.Id);
            activityFitMessages.Add(activity.Id, fitMessages);
        }

        _logger.LogInformation("Start merging fit messages");
        var mergedFitFile = new FitFileMerger([.. activityFitMessages.Values]);
        _logger.LogInformation("Finished merging fit messages");

        ActivityResponse? raceActivity = default;
        if (linkedPairedEvent != null && IsRaceEvent(linkedPairedEvent))
        {
            // activity with most tss is race
            raceActivity = activities.MaxBy(x => x.IcuTrainingLoad);
        }

        var newTssForActivities = CalculateNewTssForActivities(activities, mergedFitFile);
        foreach (var (activity, newTss) in newTssForActivities)
        {
            var updateRequest = new ActivityUpdateRequest
            {
                Type = activity.Type,
                Description = activity.Description,
                Name = activity.Name,
                Gear = new GearUpdateRequest { Id = activity.Gear.Id },
                Trainer = activity.Trainer,
                SubType = GetSubType(raceActivity, activity),
                IcuTrainingLoad = (int)Math.Round(newTss),
            };
            await _intervalsIcuHttpClient.UpdateActivity(activity.Id, updateRequest, cancellationToken);
        }

        var mergedActivityName = linkedPairedEvent?.Name
            ?? string.Join(" + ", activities.Select(x => x.Name.Trim()));
        var mergedFitSessionMessage = mergedFitFile.SessionMesgs.Single();
        var syncWithGarminModel = new SyncGarminModel
        {
            Name = mergedActivityName,
            Description = null,
            Distance = mergedFitSessionMessage.GetTotalDistance(),
            TotalElevationGain = mergedFitSessionMessage.GetTotalAscent()
        };

        return syncWithGarminModel;
    }


    private static ActivitySubType GetSubType(ActivityResponse? raceActivity, ActivityResponse activity)
    {
        if (raceActivity is null)
        {
            return ActivitySubType.None;
        }

        var raceActivityStartDate = DateTime.Parse(raceActivity.StartDate);
        var activityStartDate = DateTime.Parse(activity.StartDate);

        if (raceActivityStartDate == activityStartDate)
        {
            return ActivitySubType.Race;
        }

        if (raceActivityStartDate > activityStartDate)
        {
            return ActivitySubType.Warmup;
        }

        return ActivitySubType.Cooldown;
    }

    private static bool IsRaceEvent(EventResponse linkedPairedEvent)
    {
        return linkedPairedEvent.Tags.Contains("race");
    }


    private static List<IntervalsIcuActivityWithNewTss> CalculateNewTssForActivities(
        IReadOnlyCollection<ActivityResponse> activities,
        FitMessages mergedFitMessages)
    {
        var ftp = activities.Select(x => x.IcuFtp!.Value).Distinct().Single();

        var mergedTss = TssCalculator.Calculate(mergedFitMessages, ftp)!.Tss;
        var activitiesTss = activities.Select(x => x.PowerLoad!.Value).Sum();

        var deltaTssToAdd = mergedTss - activitiesTss;

        List<IntervalsIcuActivityWithNewTss> activitiesWithNewTss = [];
        foreach (var activity in activities)
        {
            var newTss = activity.PowerLoad!.Value
                + (activity.PowerLoad!.Value / (double)activitiesTss * deltaTssToAdd);

            activitiesWithNewTss.Add(new IntervalsIcuActivityWithNewTss(activity, newTss));
        }

        if (Math.Round(activitiesWithNewTss.Select(x => x.Tss).Sum()) != Math.Round(mergedTss))
        {
            throw new Exception("Unexpected error while calculation new tss");
        }

        return activitiesWithNewTss;
    }

    private async Task SyncWithGarmin(
        SyncGarminModel syncModel,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start sync with Garmin");

        var todayDate = DateTime.UtcNow.Date;

        var garminActivities = await _extendedGarminConnectClient
            .GetActivitiesByDate(todayDate.Date, todayDate.AddDays(1), null, cancellationToken);
        _logger.LogInformation("Got {Count} activities from Garmin", garminActivities.Length);
        if (garminActivities.Length != 1)
        {
            _logger.LogWarning("Skip syncing with Garmin, cause activities count = {Count}", garminActivities.Length);
            return;
        }

        var todaysGarminActivity = garminActivities.Single();
        if (syncModel.Distance is null || syncModel.TotalElevationGain is null)
        {
            _logger.LogWarning("Skip syncing with Garmin, no Distance or Elevation. Distance: {Distance}, Elevation: {Elevation}",
                syncModel.Distance,
                syncModel.TotalElevationGain);
            return;
        }

        var updateModel = new GarminActivityUpdateRequest
        {
            ActivityId = todaysGarminActivity.ActivityId,
            ActivityName = syncModel.Name,
            Description = syncModel.Description,
            SummaryDTO = new GarminActivityUpdateSummary
            {
                Distance = (int)syncModel.Distance,
                ElevationGain = (int)syncModel.TotalElevationGain
            }
        };

        _logger.LogInformation("Updating garmin activity with Id = {Id}", todaysGarminActivity.ActivityId);
        await _extendedGarminConnectClient.UpdateActivity(updateModel, cancellationToken);
        _logger.LogInformation("Updated garmin activity with Id = {Id}", todaysGarminActivity.ActivityId);

    }


    private record IntervalsIcuActivityWithNewTss(ActivityResponse Activity, double Tss);

    private class SyncGarminModel
    {
        public required string Name { get; init; }
        public required string? Description { get; init; }
        public required double? Distance { get; init; }
        public required double? TotalElevationGain { get; init; }
    }
}
