using FitSyncHub.Common.Applications.IntervalsIcu;
using FitSyncHub.GarminConnect.Converters;
using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.GarminConnect.Models.Responses.Workout;
using FitSyncHub.IntervalsICU;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Common;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions;

public class GarminWorkoutToIntervalsICUExporterHttpTriggerFunction
{
    private const string IntervalsIcuEventTagGarminConnect = "GarminConnect";
    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly ILogger<GarminWorkoutToIntervalsICUExporterHttpTriggerFunction> _logger;

    public GarminWorkoutToIntervalsICUExporterHttpTriggerFunction(
        GarminConnectHttpClient garminConnectHttpClient,
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        ILogger<GarminWorkoutToIntervalsICUExporterHttpTriggerFunction> logger)
    {
        _garminConnectHttpClient = garminConnectHttpClient;
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _logger = logger;
    }

    [Function(nameof(GarminWorkoutToIntervalsICUExporterHttpTriggerFunction))]
    public async Task<ActionResult> Run(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = "export-garmin-workouts-to-intervals")] HttpRequest req,
       CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Garmin to Intervals.icu export function");

        var ftp = await _garminConnectHttpClient.GetCyclingFtp(cancellationToken: cancellationToken);
        _logger.LogInformation("Retrieved FTP value from Garmin: {Ftp}", ftp);

        var trainingPlanId = await _garminConnectHttpClient.GetActiveTrainingPlanId(cancellationToken);
        _logger.LogInformation("Retrieved active training plan ID: {TrainingPlanId}", trainingPlanId);

        var trainingPlan = await _garminConnectHttpClient.GetTrainingPlan(trainingPlanId, cancellationToken);
        _logger.LogInformation("Retrieved training plan {TrainingPlan}", trainingPlan);

        var today = DateOnly.FromDateTime(DateTime.Today);
        _logger.LogInformation("Today's date: {Today}", today);

        var taskList = trainingPlan.TaskList
            .Where(x => x.TaskWorkout.SportType != null && x.TaskWorkout.AdaptiveCoachingWorkoutStatus == "NOT_COMPLETE")
            .ToList();

        _logger.LogInformation("Found {Count} uncomplete tasks in training plan", taskList.Count);

        if (taskList.Count == 0)
        {
            _logger.LogInformation("No uncomplete tasks to export");
            return new BadRequestObjectResult("No uncomplete activities");
        }

        var lastDay = taskList.MaxBy(x => x.CalendarDate)!.CalendarDate;
        _logger.LogInformation("Last calendar day among tasks {LastDay}", lastDay);

        var intervalsIcuEvents = await _intervalsIcuHttpClient.ListEvents(Constants.AthleteId, today, lastDay, cancellationToken);
        _logger.LogInformation("Retrieved {Count} existing Intervals.icu events", intervalsIcuEvents.Count);

        var garminWorkoutUuidToIntervalsIcuEventsMapping =
            await DeleteEventsAndGetGarminWorkoutUuidToIntervalsIcuEventsMapping(intervalsIcuEvents, cancellationToken);

        foreach (var workout in taskList)
        {
            var workoutId = workout.TaskWorkout.WorkoutUuid;
            _logger.LogInformation("Processing workout {WorkoutId}", workoutId);

            var workoutResponse = await _garminConnectHttpClient.GetWorkout(workoutId, cancellationToken);
            _logger.LogInformation("Retrieved Garmin workout details: {workoutId}", workoutId);

            var intervalsIcuEventStructure = GetIntervalsIcuEventStructure(ftp, workoutResponse);
            intervalsIcuEventStructure = $"GarminWorkoutUuid = {workoutId}" + Environment.NewLine + intervalsIcuEventStructure;

            var workoutName = GarminConnectToIntervalsIcuWorkoutConverter.GetGarminWorkoutTitle(workoutResponse, ftp);
            if (garminWorkoutUuidToIntervalsIcuEventsMapping.TryGetValue(workoutId, out var existingIntervalsIcuEvent))
            {
                if (existingIntervalsIcuEvent.Description == intervalsIcuEventStructure)
                {
                    _logger.LogInformation("No changes needed for existing event {ExistingIntervalsIcuEventId}", existingIntervalsIcuEvent.Id);
                    continue;
                }
                else
                {
                    _logger.LogInformation("Updating existing event by deletion: {existingIntervalsIcuEventId}", existingIntervalsIcuEvent.Id);
                    await _intervalsIcuHttpClient.DeleteEvent(Constants.AthleteId, existingIntervalsIcuEvent.Id, cancellationToken: cancellationToken);
                }
            }

            var (eventType, indoor) = workoutResponse.SportType.SportTypeKey switch
            {
                "cycling" => ("Ride", true),
                _ => ("Workout", false)
            };

            var createRequest = new CreateEventRequest
            {
                Category = EventCategory.WORKOUT,
                Description = intervalsIcuEventStructure,
                Name = workoutName,
                Indoor = indoor,
                StartDateLocal = new DateTime(workout.CalendarDate, TimeOnly.MinValue),
                SubType = ActivitySubType.NONE,
                Tags = [IntervalsIcuEventTagGarminConnect],
                Type = eventType
            };

            _logger.LogInformation("Creating new Intervals.icu event: {CreateRequest}", createRequest);
            await _intervalsIcuHttpClient.CreateEvent(
                Constants.AthleteId,
                createRequest,
                cancellationToken: cancellationToken);
        }

        _logger.LogInformation("Export function completed successfully");
        return new OkObjectResult("Ok");
    }
    private async Task<Dictionary<Guid, EventResponse>> DeleteEventsAndGetGarminWorkoutUuidToIntervalsIcuEventsMapping(
      IReadOnlyCollection<EventResponse> intervalsIcuEvents,
      CancellationToken cancellationToken)
    {
        Dictionary<Guid, EventResponse> resultDictionary = [];

        foreach (var intervalsIcuEvent in intervalsIcuEvents
            .Where(x => x.Tags?.Contains(IntervalsIcuEventTagGarminConnect) == true && x.PairedActivityId == null))
        {
            if (!TryGetGarminWorkoutUuidFromDescription(intervalsIcuEvent, out var workoutUuid))
            {
                _logger.LogInformation("Garmin workout UUID not found in description: {Description}, Deleting Event: {EventId}",
                    intervalsIcuEvent.Description,
                    intervalsIcuEvent.Id);
                await _intervalsIcuHttpClient.DeleteEvent(Constants.AthleteId, intervalsIcuEvent.Id, cancellationToken: cancellationToken);
                continue;
            }

            resultDictionary.Add(workoutUuid, intervalsIcuEvent);
        }

        return resultDictionary;
    }

    private static bool TryGetGarminWorkoutUuidFromDescription(EventResponse intervalsIcuEvent, out Guid workoutUuid)
    {
        workoutUuid = default;
        if (intervalsIcuEvent.Description == null)
        {
            return false;
        }

        var splitedDescription = intervalsIcuEvent.Description.Split("\n");
        if (splitedDescription.Length == 0)
        {
            return false;
        }

        var splitedFirstLine = splitedDescription[0].Split('=');
        if (splitedFirstLine.Length < 2)
        {
            return false;
        }

        var workoutId = splitedFirstLine[1].Trim();
        return Guid.TryParse(workoutId, out workoutUuid);
    }

    private static string GetIntervalsIcuEventStructure(int ftp,
        WorkoutResponse workoutResponse)
    {
        if (workoutResponse.SportType.SportTypeKey != "cycling")
        {
            return string.Empty;
        }

        var icuGroups = GarminConnectToIntervalsIcuWorkoutConverter
            .ConvertGarminWorkoutToIntervalsIcuWorkoutGroups(workoutResponse, ftp);

        return IntervalsIcuConverter.ConvertToIntervalsIcuFormat(icuGroups);
    }
}
