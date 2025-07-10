using Dynastream.Fit;
using FitSyncHub.Common;
using FitSyncHub.Common.Fit;
using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.GarminConnect.Models.Requests;
using FitSyncHub.GarminConnect.Models.Responses;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Common;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;
using FitSyncHub.IntervalsICU.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DateTime = System.DateTime;

namespace FitSyncHub.Functions.Functions;

public class SyncIntervalsICUWithGarminHttpTriggerFunction
{
    private readonly FitFileDecoder _decoder;
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly string _intervalsIcuAthleteId;
    private readonly ILogger<SyncIntervalsICUWithGarminHttpTriggerFunction> _logger;

    public SyncIntervalsICUWithGarminHttpTriggerFunction(
        FitFileDecoder decoder,
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        GarminConnectHttpClient garminConnectHttpClient,
        IOptions<IntervalsIcuOptions> intervalsIcuOptions,
        ILogger<SyncIntervalsICUWithGarminHttpTriggerFunction> logger)
    {
        _decoder = decoder;
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _garminConnectHttpClient = garminConnectHttpClient;
        _intervalsIcuAthleteId = intervalsIcuOptions.Value.AthleteId;
        _logger = logger;
    }

    [Function(nameof(SyncIntervalsICUWithGarminHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "sync-intervals-icu-with-garmin")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        string? countQueryParameter = req.Query["count"];
        string? dateQueryParameter = req.Query["date"];

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

        var activities = await GetRideActivities(date, cancellationToken);
        _logger.LogInformation("Received {ActivitiesCount} activities", activities.Count);

        if (activities.Count != count)
        {
            _logger.LogInformation("Found {ActivitiesCount} todays intervals.icu activities, but specified {ParsedCount} in request", activities.Count, count);
            return new BadRequestObjectResult($"Found {activities.Count} todays intervals.icu activities, but specified {count} in request");
        }

        ActivitySummary activitySummary;
        if (activities.Count == 1)
        {
            var activity = activities.Single();

            activitySummary = new ActivitySummary
            {
                Name = activity.Name,
                Description = activity.Description,
                Distance = activity.Distance,
                ElevationAscent = activity.TotalElevationGain!.Value,
            };
        }
        else
        {
            var pairedEvent = await GetPairedEvent(activities, date, cancellationToken);

            activitySummary
                = await UpdateActivitiesWithNewTssAndReturnSummary(activities, pairedEvent, cancellationToken);
        }

        var garminActivity = await UpdateGarminSummaryWithIntervalsData(date, activitySummary, cancellationToken);
        var intervalsActivities = await GetRideActivities(date, cancellationToken);
        await UpdateIntervalsIcuActivitiesWithGarminData(garminActivity, intervalsActivities, cancellationToken);

        return new OkObjectResult("Success");
    }

    private async Task UpdateIntervalsIcuActivitiesWithGarminData(
        GarminActivityResponse? garminActivity,
        IReadOnlyCollection<ActivityResponse> intervalsActivities,
        CancellationToken cancellationToken)
    {
        if (garminActivity is null)
        {
            return;
        }

        var garminRpe = garminActivity.SummaryDTO?.DirectWorkoutRpe;
        var garminFeel = garminActivity.SummaryDTO?.DirectWorkoutFeel;

        if (!garminRpe.HasValue || !garminFeel.HasValue)
        {
            _logger.LogWarning("Garmin activity does not have RPE or Feel values");
            return;
        }

        var (intervalsRpe, intervalsFeel) = ConvertGarminValuesToIntervalsIcu(garminRpe.Value, garminFeel.Value);

        foreach (var intervalsActivity in intervalsActivities)
        {
            if (intervalsRpe != intervalsActivity.IcuRpe || intervalsFeel != intervalsActivity.Feel)
            {
                var updateRequest = new ActivityUpdateRequest
                {
                    IcuRpe = intervalsRpe,
                    Feel = intervalsFeel,
                };

                await _intervalsIcuHttpClient.UpdateActivity(intervalsActivity.Id, updateRequest, cancellationToken);
            }
        }

        static (int rpe, int feel) ConvertGarminValuesToIntervalsIcu(int garminRpe, int garminFeel)
        {
            var rpe = garminRpe / 10;
            var feel = garminFeel switch
            {
                0 => 5,
                25 => 4,
                50 => 3,
                75 => 2,
                100 => 1,
                _ => throw new NotImplementedException(),
            };

            return (rpe, feel);
        }
    }

    private async Task<IReadOnlyCollection<ActivityResponse>> GetRideActivities(DateOnly date, CancellationToken cancellationToken)
    {
        var activities = await _intervalsIcuHttpClient.ListActivities(_intervalsIcuAthleteId,
                    new DateTime(date, TimeOnly.MinValue), new DateTime(date, TimeOnly.MaxValue), 10, cancellationToken) ?? [];
        return [.. activities.Where(x => x.Type.Contains("Ride"))];
    }

    private async Task<ActivitySummary> UpdateActivitiesWithNewTssAndReturnSummary(
        IReadOnlyCollection<ActivityResponse> activities,
        EventResponse? pairedEvent,
        CancellationToken cancellationToken)
    {
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
        var mergedFitFile = new SequentialFitFileMerger([.. activityFitMessages.Values]);
        _logger.LogInformation("Finished merging fit messages");

        ActivityResponse? raceActivity = default;
        if (pairedEvent != null && IsRaceEvent(pairedEvent))
        {
            // activity with most intensity is race
            raceActivity = activities.MaxBy(x => x.IcuIntensity);
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

        var mergedActivityName = pairedEvent?.Name
            ?? string.Join(" + ", activities.Select(x => x.Name.Trim()));
        var mergedFitSessionMessage = mergedFitFile.SessionMesgs.Single();
        return new ActivitySummary
        {
            Name = mergedActivityName,
            Description = null,
            Distance = mergedFitSessionMessage.GetTotalDistance(),
            ElevationAscent = mergedFitSessionMessage.GetTotalAscent()
        };
    }

    private async Task<EventResponse?> GetPairedEvent(
        IReadOnlyCollection<ActivityResponse> activities,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var activityWithLinkedPairedEvent = activities.SingleOrDefault(x => x.PairedEventId.HasValue);
        if (activityWithLinkedPairedEvent?.PairedEventId is { } pairedEventId)
        {
            _logger.LogInformation("Get linked paired event");
            var pairedEvent = await _intervalsIcuHttpClient.GetEvent(_intervalsIcuAthleteId, pairedEventId, cancellationToken);

            _logger.LogInformation("Unlinking paired event {EventId} from activity {ActivityId}", pairedEvent, activityWithLinkedPairedEvent.Id);
            await _intervalsIcuHttpClient.UnlinkPairedWorkout(activityWithLinkedPairedEvent.Id, cancellationToken);

            return pairedEvent;
        }

        var events = await _intervalsIcuHttpClient.ListEvents(
            _intervalsIcuAthleteId,
            new ListEventsQueryParams(date, date),
            cancellationToken);

        return events
            .SingleOrDefault(x => x.Type.Contains("Ride"));
    }

    private static ActivitySubType GetSubType(ActivityResponse? raceActivity, ActivityResponse activity)
    {
        if (raceActivity is null)
        {
            return ActivitySubType.None;
        }

        var raceActivityStartDate = raceActivity.StartDate;
        var activityStartDate = activity.StartDate;

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
        return linkedPairedEvent.Tags is { } && linkedPairedEvent.Tags.Contains("race");
    }

    private static List<IntervalsIcuActivityWithNewTss> CalculateNewTssForActivities(
        IReadOnlyCollection<ActivityResponse> activities,
        FitMessages mergedFitMessages)
    {
        var ftp = activities.Select(x => x.IcuFtp!.Value).Distinct().Single();

        var mergedTss = TssCalculator.Calculate(mergedFitMessages, ftp)!.Tss;
        var activitiesTss = activities.Sum(x => x.PowerLoad!.Value);

        var deltaTssToAdd = mergedTss - activitiesTss;

        List<IntervalsIcuActivityWithNewTss> activitiesWithNewTss = [];
        foreach (var activity in activities)
        {
            var newTss = activity.PowerLoad!.Value
                + (activity.PowerLoad!.Value / (double)activitiesTss * deltaTssToAdd);

            activitiesWithNewTss.Add(new IntervalsIcuActivityWithNewTss(activity, newTss));
        }

        if (Math.Round(activitiesWithNewTss.Sum(x => x.Tss)) != Math.Round(mergedTss))
        {
            throw new Exception("Unexpected error while calculation new tss");
        }

        return activitiesWithNewTss;
    }

    private async Task<GarminActivityResponse?> UpdateGarminSummaryWithIntervalsData(
        DateOnly date,
        ActivitySummary activitySummary,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start sync with Garmin");

        var garminActivities = await _garminConnectHttpClient.GetActivitiesByDate(
            new DateTime(date, TimeOnly.MinValue),
            new DateTime(date, TimeOnly.MaxValue),
            "cycling",
            cancellationToken);
        _logger.LogInformation("Got {Count} activities from Garmin", garminActivities.Count);
        if (garminActivities.Count != 1)
        {
            throw new InvalidDataException($"Skip syncing with Garmin, cause activities count = {garminActivities.Count}");
        }

        var todaysGarminActivity = garminActivities.Single();
        if (activitySummary.Distance is null || activitySummary.ElevationAscent is null)
        {
            _logger.LogWarning("Skip syncing with Garmin, no Distance or Elevation. Distance: {Distance}, Elevation ascent: {ElevationAscent}",
                activitySummary.Distance,
                activitySummary.ElevationAscent);
            return default;
        }

        var activityId = todaysGarminActivity.ActivityId;

        var updateModel = new GarminActivityUpdateRequest
        {
            ActivityId = activityId,
            ActivityName = activitySummary.Name,
            Description = activitySummary.Description,
            SummaryDTO = new GarminActivityUpdateSummary
            {
                Distance = (int)activitySummary.Distance,
                ElevationGain = (int)activitySummary.ElevationAscent,
            }
        };

        _logger.LogInformation("Updating garmin activity with Id = {Id}", activityId);
        await _garminConnectHttpClient.UpdateActivity(updateModel, cancellationToken);
        _logger.LogInformation("Updated garmin activity with Id = {Id}", activityId);

        return await _garminConnectHttpClient.GetActivity(activityId, cancellationToken);
    }

    private record IntervalsIcuActivityWithNewTss(ActivityResponse Activity, double Tss);

    private record ActivitySummary
    {
        public required string Name { get; init; }
        public required string? Description { get; init; }
        public required double? Distance { get; init; }
        public required double? ElevationAscent { get; init; }
    }
}
