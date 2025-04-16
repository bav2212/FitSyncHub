using System.Text;
using FitSyncHub.Common.Applications.IntervalsIcu;
using FitSyncHub.GarminConnect.Converters;
using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.GarminConnect.Models.Responses.TrainingPlan;
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

        var garminTrainingPlanTaskList = trainingPlan.TaskList
            .Where(x => x.TaskWorkout.SportType != null && x.TaskWorkout.AdaptiveCoachingWorkoutStatus == "NOT_COMPLETE")
            .ToList();

        _logger.LogInformation("Found {Count} uncomplete tasks in training plan", garminTrainingPlanTaskList.Count);

        if (garminTrainingPlanTaskList.Count == 0)
        {
            _logger.LogInformation("No uncomplete tasks to export");
            return new BadRequestObjectResult("No uncomplete activities");
        }

        var lastDay = garminTrainingPlanTaskList.MaxBy(x => x.CalendarDate)!.CalendarDate;
        _logger.LogInformation("Last calendar day among tasks {LastDay}", lastDay);

        var intervalsIcuEvents = await _intervalsIcuHttpClient.ListEvents(Constants.AthleteId, today, lastDay, cancellationToken);
        _logger.LogInformation("Retrieved {Count} existing Intervals.icu events", intervalsIcuEvents.Count);

        var garminWorkoutUuidToIntervalsIcuEventsMapping = await DeleteOutdatedEventsAndGetGarminWorkoutUuidToIntervalsIcuEventsMapping(
            intervalsIcuEvents, garminTrainingPlanTaskList, cancellationToken);

        foreach (var workout in garminTrainingPlanTaskList)
        {
            var workoutId = workout.TaskWorkout.WorkoutUuid;
            _logger.LogInformation("Processing workout {WorkoutId}", workoutId);

            var workoutResponse = await _garminConnectHttpClient.GetWorkout(workoutId, cancellationToken);
            _logger.LogInformation("Retrieved Garmin workout details: {workoutId}", workoutId);

            var intervalsIcuEventStructure = IntervalsICUDescriptionHelper
                .GenerateDescriptionBlock(workoutId, GetIntervalsIcuEventStructure(ftp, workoutResponse));

            var workoutName = GarminConnectToIntervalsIcuWorkoutConverter.GetGarminWorkoutTitle(workoutResponse, ftp);
            if (garminWorkoutUuidToIntervalsIcuEventsMapping.TryGetValue(workoutId, out var existingIntervalsIcuEvent))
            {
                if (existingIntervalsIcuEvent.Description is { } && HasSameGeneratedContent(intervalsIcuEventStructure, existingIntervalsIcuEvent.Description))
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

    private static bool HasSameGeneratedContent(string generatedDescription, string existingDescription)
    {
        var existingDescriptionParts = IntervalsICUDescriptionHelper.ExtractMainBlock(existingDescription);
        var existingIntervalsIcuEventStructure = IntervalsICUDescriptionHelper.ExtractMainBlock(generatedDescription);

        return existingDescriptionParts.SequenceEqual(existingIntervalsIcuEventStructure);
    }

    private async Task<Dictionary<Guid, EventResponse>> DeleteOutdatedEventsAndGetGarminWorkoutUuidToIntervalsIcuEventsMapping(
      IReadOnlyCollection<EventResponse> intervalsIcuEvents,
      List<TrainingPlanTaskItemResponse> garminTrainingPlanTaskList,
      CancellationToken cancellationToken)
    {
        var garminWorkoutIds = garminTrainingPlanTaskList.Select(x => x.TaskWorkout.WorkoutUuid).ToHashSet();

        Dictionary<Guid, EventResponse> resultDictionary = [];

        foreach (var intervalsIcuEvent in intervalsIcuEvents
            .Where(x => x.Tags?.Contains(IntervalsIcuEventTagGarminConnect) == true && x.PairedActivityId == null))
        {
            if (!IntervalsICUDescriptionHelper
                .TryGetGarminWorkoutUuidFromDescription(intervalsIcuEvent.Description, out var workoutUuid))
            {
                _logger.LogInformation("Garmin workout UUID not found in description: {Description}, Deleting Event: {EventId}",
                   intervalsIcuEvent.Description,
                   intervalsIcuEvent.Id);
                await _intervalsIcuHttpClient.DeleteEvent(Constants.AthleteId, intervalsIcuEvent.Id, cancellationToken: cancellationToken);
                continue;
            }

            if (!garminWorkoutIds.Contains(workoutUuid))
            {
                _logger.LogInformation("Garmin workouts does not contain UUID {GarminWorkoutUUID}, Deleting Event: {EventId}",
                    workoutUuid,
                    intervalsIcuEvent.Id);
                await _intervalsIcuHttpClient.DeleteEvent(Constants.AthleteId, intervalsIcuEvent.Id, cancellationToken: cancellationToken);
                continue;
            }

            resultDictionary.Add(workoutUuid, intervalsIcuEvent);
        }

        return resultDictionary;
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

    private static class IntervalsICUDescriptionHelper
    {
        private const string IntervalsIcuDescriptionAutogeneratedBlockStart = "<!-- BEGIN AUTOGENERATED BLOCK -->";
        private const string IntervalsIcuDescriptionAutogeneratedBlockEnd = "<!-- END AUTOGENERATED BLOCK -->";
        private const string GarminWorkoutUuidBlockStart = "GarminWorkoutUuid = ";

        public static string GenerateDescriptionBlock(Guid workoutId, string intervalsIcuWorkoutStructure)
        {
            var sb = new StringBuilder();
            sb.AppendLine(IntervalsIcuDescriptionAutogeneratedBlockStart);
            sb.AppendLine($"{GarminWorkoutUuidBlockStart}{workoutId}");
            sb.AppendLine(intervalsIcuWorkoutStructure);
            sb.AppendLine(IntervalsIcuDescriptionAutogeneratedBlockEnd);

            return sb.ToString();
        }

        public static bool TryGetGarminWorkoutUuidFromDescription(string? description, out Guid workoutUuid)
        {
            workoutUuid = default;
            if (description == null)
            {
                return false;
            }

            var mainBlockLines = ExtractMainBlock(description);
            if (mainBlockLines.Count == 0)
            {
                return false;
            }

            var splitedFirstLine = mainBlockLines[0].Split(GarminWorkoutUuidBlockStart);
            if (splitedFirstLine.Length < 2)
            {
                return false;
            }

            var workoutId = splitedFirstLine[1].Trim();
            return Guid.TryParse(workoutId, out workoutUuid);
        }

        public static List<string> ExtractMainBlock(string intervalsIcuDescription)
        {
            var lines = intervalsIcuDescription.Split('\n', StringSplitOptions.TrimEntries);
            var insideBlock = false;
            var result = new List<string>();

            foreach (var line in lines)
            {
                if (line.Trim() == IntervalsIcuDescriptionAutogeneratedBlockStart)
                {
                    insideBlock = true;
                    continue;
                }
                if (line.Trim() == IntervalsIcuDescriptionAutogeneratedBlockEnd)
                {
                    insideBlock = false;
                    continue;
                }

                if (insideBlock && !string.IsNullOrWhiteSpace(line))
                {
                    result.Add(line);
                }
            }

            return result;
        }
    }
}
