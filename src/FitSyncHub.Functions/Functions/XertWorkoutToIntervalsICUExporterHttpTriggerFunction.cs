using System.Text;
using FitSyncHub.Functions.Helpers;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Common;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.Options;
using FitSyncHub.Xert;
using FitSyncHub.Xert.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Functions.Functions;

public class XertWorkoutToIntervalsICUExporterHttpTriggerFunction
{
    private readonly IXertHttpClient _xertHttpClient;
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly string _athleteId;
    private readonly ILogger<XertWorkoutToIntervalsICUExporterHttpTriggerFunction> _logger;

    public XertWorkoutToIntervalsICUExporterHttpTriggerFunction(
       IXertHttpClient xertHttpClient,
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        IOptions<IntervalsIcuOptions> intervalsIcuOptions,
        ILogger<XertWorkoutToIntervalsICUExporterHttpTriggerFunction> logger)
    {
        _xertHttpClient = xertHttpClient;
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _athleteId = intervalsIcuOptions.Value.AthleteId;
        _logger = logger;
    }

    [Function(nameof(XertWorkoutToIntervalsICUExporterHttpTriggerFunction))]
    public async Task<ActionResult> Run(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = "export-xert-workouts-to-intervals")] HttpRequest req,
       CancellationToken cancellationToken)
    {
        _ = req;

        _logger.LogInformation("Starting Xert to Intervals.icu export function");

        var ti = await _xertHttpClient.GetTrainingInfo(XertWorkoutFormat.ZWO, cancellationToken);
        _logger.LogInformation("Retrieved training info value from Xert");

        var zwo = await _xertHttpClient.GetDownloadWorkout(ti.Wotd.Url, cancellationToken);
        _logger.LogInformation("Downloaded workout zwo from Xert");

        var base64EncodedWorkoutStructure = Convert.ToBase64String(Encoding.UTF8.GetBytes(zwo));

        var createdEvent = await _intervalsIcuHttpClient.CreateEvent(_athleteId, new CreateEventFromFileRequest
        {
            Category = EventCategory.Workout,
            Type = EventType.Ride,
            StartDateLocal = DateTime.Now.Date,
            FileContentsBase64 = base64EncodedWorkoutStructure
        }, default);

        var intervalsIcuFutureGarminEventsOverview = ResponseOverviewHelper.IntervalsIcuEventsResponseOverview(createdEvent);

        return new OkObjectResult(intervalsIcuFutureGarminEventsOverview);
    }
}
