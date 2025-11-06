using System.Text;
using FitSyncHub.Functions.Helpers;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Common;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.Xert;
using FitSyncHub.Xert.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions;

public class XertWorkoutToIntervalsICUExporterHttpTriggerFunction
{
    private const string IntervalsIcuEventTagXert = "Xert";

    private readonly IXertHttpClient _xertHttpClient;
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly ILogger<XertWorkoutToIntervalsICUExporterHttpTriggerFunction> _logger;

    public XertWorkoutToIntervalsICUExporterHttpTriggerFunction(
        IXertHttpClient xertHttpClient,
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        ILogger<XertWorkoutToIntervalsICUExporterHttpTriggerFunction> logger)
    {
        _xertHttpClient = xertHttpClient;
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _logger = logger;
    }

    [Function(nameof(XertWorkoutToIntervalsICUExporterHttpTriggerFunction))]
    public async Task<ActionResult> Run(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = "export-xert-workouts-to-intervals")] HttpRequest req,
       CancellationToken cancellationToken)
    {
        _ = req;

        _logger.LogInformation("Starting Xert to Intervals.icu export function");

        var today = DateTime.Now.Date;

        var ti = await _xertHttpClient.GetTrainingInfo(XertWorkoutFormat.ZWO, cancellationToken);
        _logger.LogInformation("Retrieved training info value from Xert");

        var workoutOfTheDayUrl = ti.WorkoutOfTheDay.Url;
        if (workoutOfTheDayUrl is null)
        {
            return new OkObjectResult("Nothing to export, no xert workouts for today");
        }

        var zwo = await _xertHttpClient.GetDownloadWorkout(workoutOfTheDayUrl, cancellationToken);
        _logger.LogInformation("Downloaded workout zwo from Xert");

        var intervalsIcuEvents = await _intervalsIcuHttpClient.ListEvents(new(today, today), cancellationToken);
        _logger.LogInformation("Retrieved {Count} existing Intervals.icu events", intervalsIcuEvents.Count);

        var intervalsIcuEventsMapping = intervalsIcuEvents
            .Where(x => x.Tags?.Contains(IntervalsIcuEventTagXert) == true && x.PairedActivityId == null)
            .ToDictionary(x => x.Name);

        if (ti.WorkoutOfTheDay.Name is { } && intervalsIcuEventsMapping.ContainsKey(ti.WorkoutOfTheDay.Name))
        {
            return new OkObjectResult("Event already exists.");
        }

        var base64EncodedWorkoutStructure = Convert.ToBase64String(Encoding.UTF8.GetBytes(zwo));

        var createdEvent = await _intervalsIcuHttpClient.CreateEvent(new CreateEventFromFileRequest
        {
            Category = EventCategory.Workout,
            Type = EventType.Ride,
            StartDateLocal = today,
            FileContentsBase64 = base64EncodedWorkoutStructure,
            Tags = [IntervalsIcuEventTagXert],
        }, default);

        var intervalsIcuFutureGarminEventsOverview = IntervalsIcuResponseOverviewHelper.ToStringOverview(createdEvent);

        return new OkObjectResult(intervalsIcuFutureGarminEventsOverview);
    }
}
