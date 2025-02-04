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

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activities = await _intervalsIcuHttpClient.ListActivities(Constants.AthleteId,
            new DateTime(today, TimeOnly.MinValue), new DateTime(today, TimeOnly.MaxValue), 10, cancellationToken) ?? [];
        _logger.LogInformation("Received {ActivitiesCount} today's activities", activities.Count);

        if (activities.Count != count)
        {
            _logger.LogInformation("Found {ActivitiesCount} todays activities, but specified {ParsedCount} in request", activities.Count, count);
            return new BadRequestObjectResult($"Found {activities.Count} todays activities, but specified {count} in request");
        }

        string activityIdToSyncWithGarmin;
        if (activities.Count == 1)
        {
            activityIdToSyncWithGarmin = activities.Single().Id;
        }
        else
        {
            activityIdToSyncWithGarmin = await MergeActivities(activities, cancellationToken);
        }

        if (bool.TryParse(syncWithGarminQueryParameter, out var syncWithGarmin) && syncWithGarmin)
        {
            await SyncWithGarmin(activityIdToSyncWithGarmin, cancellationToken);
        }

        return new OkObjectResult("Success");
    }

    private async Task<string> MergeActivities(IReadOnlyCollection<ActivityResponse> activities, CancellationToken cancellationToken)
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

        Dictionary<string, Dynastream.Fit.FitMessages> activityFitMessages = [];
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
        var result = new FitFileMerger([.. activityFitMessages.Values]);
        _logger.LogInformation("Finished merging fit messages");

        byte[] mergedFileBytes;
        using (var ms = new MemoryStream())
        {
            _logger.LogInformation("Start encoding merged fit messages");
            _encoder.Encode(ms, result);
            _logger.LogInformation("Finished encoding merged fit messages");
            mergedFileBytes = ms.ToArray();
        }

        var mergedActivityName = GetMergedEventName(activities, linkedPairedEvent);

        _logger.LogInformation("Creating merged activity");
        var createActivityResponse = await _intervalsIcuHttpClient.CreateActivity(Constants.AthleteId, mergedFileBytes,
            name: mergedActivityName,
            pairedEventId: linkedPairedEvent?.Id,
            cancellationToken: cancellationToken);
        _logger.LogInformation("Created merged activity, ActivityId: {ActivityId}", createActivityResponse.Id);


        if (linkedPairedEvent != null && IsRaceEvent(linkedPairedEvent))
        {
            _logger.LogInformation("Update activity {ActivityId} to set Race = true", createActivityResponse.Id);

            var activity = await _intervalsIcuHttpClient.GetActivity(createActivityResponse.Id, cancellationToken);

            var updateRequest = new ActivityUpdateRequest
            {
                Type = activity.Type,
                Commute = activity.Commute,
                Description = activity.Description,
                Name = activity.Name,
                Gear = new GearUpdateRequest { Id = activity.Gear.Id },
                Trainer = activity.Trainer,
                Race = true
            };

            await _intervalsIcuHttpClient.UpdateActivity(activity.Id, updateRequest, cancellationToken);
            _logger.LogInformation("Updated activity {ActivityId} to set Race = true", createActivityResponse.Id);
        }

        foreach (var activity in activities)
        {
            _logger.LogInformation("Start deleting activity {ActivityId} from Intervals.icu", activity.Id);
            await _intervalsIcuHttpClient.DeleteActivity(activity.Id, cancellationToken);
            _logger.LogInformation("Finished deleting activity {ActivityId} from Intervals.icu", activity.Id);
        }

        return createActivityResponse.Id;
    }

    private static string? GetMergedEventName(IEnumerable<ActivityResponse> activities,
    EventResponse? linkedPairedEvent)
    {
        if (linkedPairedEvent is null)
        {
            return default;
        }

        var eventName = linkedPairedEvent.Name.Trim();
        if (IsRaceEvent(linkedPairedEvent))
        {
            return $"Warmup + {eventName} + cooldown";
        }

        return eventName;
    }

    private static bool IsRaceEvent(EventResponse linkedPairedEvent)
    {
        return linkedPairedEvent.Tags.Contains("race");
    }

    private async Task SyncWithGarmin(string intervalsIcuActivityId, CancellationToken cancellationToken)
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

        var intervalsIcuActivity = await _intervalsIcuHttpClient.GetActivity(intervalsIcuActivityId, cancellationToken);
        _logger.LogInformation("Got activity from Intervals.ICU");

        if (intervalsIcuActivity.Distance is null || intervalsIcuActivity.TotalElevationGain is null)
        {
            _logger.LogWarning("Skip syncing with Garmin, intervals.icu activity has no Distance or Elevation. Distance: {Distance}, Elevation: {Elevation}",
                intervalsIcuActivity.Distance,
                intervalsIcuActivity.TotalElevationGain);
            return;
        }

        var updateModel = new GarminActivityUpdateRequest
        {
            ActivityId = todaysGarminActivity.ActivityId,
            ActivityName = intervalsIcuActivity.Name,
            Description = intervalsIcuActivity.Description,
            SummaryDTO = new GarminActivityUpdateSummary
            {
                Distance = (int)intervalsIcuActivity.Distance,
                ElevationGain = (int)intervalsIcuActivity.TotalElevationGain
            }
        };

        _logger.LogInformation("Updating garmin activity with Id = {Id}", todaysGarminActivity.ActivityId);
        await _extendedGarminConnectClient.UpdateActivity(updateModel, cancellationToken);
        _logger.LogInformation("Updated garmin activity with Id = {Id}", todaysGarminActivity.ActivityId);

    }
}
