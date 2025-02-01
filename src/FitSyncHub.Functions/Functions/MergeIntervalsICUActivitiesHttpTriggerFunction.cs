using FitSyncHub.Common.Fit;
using FitSyncHub.IntervalsICU;
using FitSyncHub.IntervalsICU.HttpClients;
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
    private readonly ILogger<MergeIntervalsICUActivitiesHttpTriggerFunction> _logger;

    public MergeIntervalsICUActivitiesHttpTriggerFunction(
        FitFileDecoder decoder,
        FitFileEncoder encoder,
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        ILogger<MergeIntervalsICUActivitiesHttpTriggerFunction> logger)
    {
        _decoder = decoder;
        _encoder = encoder;
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _logger = logger;
    }

    [Function(nameof(MergeIntervalsICUActivitiesHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "merge-intervals-icu-activities")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        string? count = req.Query["count"];

        if (count is null)
        {
            return new BadRequestObjectResult("wrong request");
        }

        if (!int.TryParse(count, out var parsedCount))
        {
            return new BadRequestObjectResult("Count has wrong format");
        }

        if (parsedCount > 10)
        {
            return new BadRequestObjectResult("Can't parse more that 10 activities");
        }

        var activities = await _intervalsIcuHttpClient
            .ListActivities(Constants.AthleteId, DateTime.Now.AddDays(-1), DateTime.Now, 10, cancellationToken) ?? [];
        _logger.LogInformation("Received {ActivitiesCount} today's activities", activities.Count);

        if (activities.Count != parsedCount)
        {
            return new BadRequestObjectResult($"Found {activities.Count} todays activities, but specified {parsedCount} in request");
        }

        int? linkedPairedEvent = default;
        var activityWithLinkedPairedEvent = activities.Where(x => x.PairedEventId.HasValue).SingleOrDefault();
        if (activityWithLinkedPairedEvent != null)
        {
            _logger.LogInformation("Unlinking paired event {EventId} from activity {ActivityId}", linkedPairedEvent, activityWithLinkedPairedEvent.Id);
            linkedPairedEvent = activityWithLinkedPairedEvent.PairedEventId;
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

        _logger.LogInformation("Creating merged activity");
        var createActivityResponse = await _intervalsIcuHttpClient.CreateActivity(Constants.AthleteId, mergedFileBytes,
            pairedEventId: linkedPairedEvent,
            cancellationToken: cancellationToken);
        _logger.LogInformation("Created merged activity, ActivityId: {ActivityId}", createActivityResponse.Id);

        foreach (var activity in activities)
        {
            _logger.LogInformation("Start deleting activity {ActivityId} from Intervals.icu", activity.Id);
            await _intervalsIcuHttpClient.DeleteActivity(activity.Id, cancellationToken);
            _logger.LogInformation("Finished deleting activity {ActivityId} from Intervals.icu", activity.Id);
        }

        return new OkResult();
    }
}
