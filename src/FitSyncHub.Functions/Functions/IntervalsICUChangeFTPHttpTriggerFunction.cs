using Dynastream.Fit;
using FitSyncHub.Common;
using FitSyncHub.Common.Extensions;
using FitSyncHub.Common.Fit;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using DateTime = System.DateTime;

namespace FitSyncHub.Functions.Functions;

public sealed class IntervalsICUChangeFTPHttpTriggerFunction
{
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly FitFileDecoder _decoder;
    private readonly ILogger<IntervalsICUChangeFTPHttpTriggerFunction> _logger;

    public IntervalsICUChangeFTPHttpTriggerFunction(
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        FitFileDecoder decoder,
        ILogger<IntervalsICUChangeFTPHttpTriggerFunction> logger)
    {
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _decoder = decoder;
        _logger = logger;
    }

#if !DEBUG
    [Function(nameof(IntervalsICUChangeFTPHttpTriggerFunction))]
#endif
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "intervals-icu-change-ftp")] HttpRequest req,
        CancellationToken cancellationToken)
    {
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

        string? newFtpQueryParameter = req.Query["ftp"];
        if (!uint.TryParse(newFtpQueryParameter, out var ftp))
        {
            return new BadRequestObjectResult("Invalid or missing 'ftp' query parameter");
        }

        _logger.LogInformation("C# HTTP trigger function processed a request.");
        const int Limit = 100;
        var oldest = new DateTime(fromDateOnly, TimeOnly.MinValue);
        var newest = new DateTime(toDateOnly, TimeOnly.MaxValue);
        List<ActivityResponse?> activitiesPortion = [.. await _intervalsIcuHttpClient
            .ListActivities(new (oldest, newest) { Limit = Limit }, cancellationToken)];

        if (activitiesPortion.Count == Limit)
        {
            throw new InvalidOperationException("Too many activities in the given date range, please narrow down the range");
        }

        var activities = activitiesPortion
            // skip strava activities
            .WhereNotNull()
            .Where(x => x.IsRide)
            .ToList();

        if (!activities.Any(x => x.StartDateLocal.Date == oldest))
        {
            throw new InvalidOperationException("No activities found for the oldest date");
        }

        await Implementation(activities, ftp, cancellationToken);

        return new OkObjectResult("Success");
    }

    private async Task Implementation(
        List<ActivityResponse> activities,
        uint ftp,
        CancellationToken cancellationToken)
    {
        var activityGroups = activities
            .GroupBy(x => x.StartDate.Date).ToDictionary(x => x.Key, x => x.ToList());

        foreach (var (date, activitiesForDate) in activityGroups)
        {
            if (activitiesForDate.Count == 1)
            {
                await UpdateFtp(activitiesForDate.Single(), ftp, cancellationToken);
            }

            await UpdateActivitiesFtp(activitiesForDate, ftp, cancellationToken);
        }
    }

    private async Task UpdateActivitiesFtp(
        IReadOnlyCollection<ActivityResponse> activities,
        uint ftp,
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

        var newTssForActivities = CalculateNewTssForActivities(activities, ftp, mergedFitFile);
        foreach (var (activity, newTss) in newTssForActivities)
        {
            var updateRequest = new ActivityUpdateRequest
            {
                IcuTrainingLoad = (uint)Math.Round(newTss),
                IcuFtp = ftp
            };
            await _intervalsIcuHttpClient.UpdateActivity(activity.Id, updateRequest, cancellationToken);
        }
        return;
    }

    private static List<IntervalsIcuActivityWithNewTss> CalculateNewTssForActivities(
       IReadOnlyCollection<ActivityResponse> activities,
       uint ftp,
       FitMessages mergedFitMessages)
    {
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

    private sealed record IntervalsIcuActivityWithNewTss(ActivityResponse Activity, double Tss);

    private async Task UpdateFtp(
        ActivityResponse activityResponse,
        uint ftp,
        CancellationToken cancellationToken)
    {
        if (activityResponse.PowerLoad != activityResponse.IcuTrainingLoad)
        {
            throw new InvalidOperationException(
                $"Activity {activityResponse.Id} has different PowerLoad and IcuTrainingLoad");
        }

        await _intervalsIcuHttpClient.UpdateActivity(activityResponse.Id, new ActivityUpdateRequest
        {
            IcuFtp = ftp
        }, cancellationToken);
    }
}
