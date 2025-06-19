using System.Globalization;
using System.Text;
using FitSyncHub.GarminConnect.Converters;
using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.GarminConnect.Services;
using FitSyncHub.IntervalsICU.Builders;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Common;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Functions.Functions;

public class GarminWorkoutToIntervalsICUExporterHttpTriggerFunction
{
    private const string IntervalsIcuEventTagGarminConnect = "GarminConnect";
    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly GarminConnectToInternalWorkoutConverterService _converterService;
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly string _athleteId;
    private readonly ILogger<GarminWorkoutToIntervalsICUExporterHttpTriggerFunction> _logger;

    public GarminWorkoutToIntervalsICUExporterHttpTriggerFunction(
        GarminConnectHttpClient garminConnectHttpClient,
        GarminConnectToInternalWorkoutConverterService converterService,
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        IOptions<IntervalsIcuOptions> intervalsIcuOptions,
        ILogger<GarminWorkoutToIntervalsICUExporterHttpTriggerFunction> logger)
    {
        _garminConnectHttpClient = garminConnectHttpClient;
        _converterService = converterService;
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _athleteId = intervalsIcuOptions.Value.AthleteId;
        _logger = logger;
    }

    [Function(nameof(GarminWorkoutToIntervalsICUExporterHttpTriggerFunction))]
    public async Task<ActionResult> Run(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = "export-garmin-workouts-to-intervals")] HttpRequest req,
       CancellationToken cancellationToken)
    {
        _ = req;

        string? dateQueryParameter = req.Query["date"];
        var date = DateOnly.TryParse(dateQueryParameter, out var dateParsed) ? dateParsed : default;

        _logger.LogInformation("Starting Garmin to Intervals.icu export function");

        var ftp = await _garminConnectHttpClient.GetCyclingFtp(cancellationToken: cancellationToken);
        _logger.LogInformation("Retrieved FTP value from Garmin: {Ftp}", ftp);

        var trainingPlanId = await _garminConnectHttpClient.GetActiveTrainingPlanId(cancellationToken);
        _logger.LogInformation("Retrieved active training plan ID: {TrainingPlanId}", trainingPlanId);

        var trainingPlan = await _garminConnectHttpClient.GetTrainingPlan(trainingPlanId, cancellationToken);
        _logger.LogInformation("Retrieved training plan {TrainingPlan}", trainingPlan);

        var garminTrainingPlanTaskList = trainingPlan.TaskList
            .Where(x => x.TaskWorkout.SportType != null && x.TaskWorkout.AdaptiveCoachingWorkoutStatus == "NOT_COMPLETE"
                // filter by date if provided
                && (date == default || x.CalendarDate == date))
            .ToList();

        _logger.LogInformation("Found {Count} incomplete tasks in training plan", garminTrainingPlanTaskList.Count);

        if (garminTrainingPlanTaskList.Count == 0)
        {
            _logger.LogInformation("No incomplete tasks to export");
            return new BadRequestObjectResult("No incomplete activities");
        }

        var garminTrainingPlanTaskListDates = garminTrainingPlanTaskList.Select(x => x.CalendarDate).Order().ToArray();

        var firstDay = garminTrainingPlanTaskListDates[0];
        var lastDay = garminTrainingPlanTaskListDates[^1];
        _logger.LogInformation("Last calendar day among tasks {LastDay}", lastDay);

        var intervalsIcuEvents = await _intervalsIcuHttpClient.ListEvents(_athleteId,
            new ListEventsQueryParams(firstDay, lastDay), cancellationToken);
        _logger.LogInformation("Retrieved {Count} existing Intervals.icu events", intervalsIcuEvents.Count);

        var intervalsIcuEventsMapping = intervalsIcuEvents
            .Where(x => x.Tags?.Contains(IntervalsIcuEventTagGarminConnect) == true && x.PairedActivityId == null)
            .ToDictionary(x => new IntervalsIcuMappingKey
            {
                Date = DateOnly.FromDateTime(x.StartDateLocal),
                Title = x.Name
            });

        foreach (var workout in garminTrainingPlanTaskList)
        {
            var workoutId = workout.TaskWorkout.WorkoutUuid;
            _logger.LogInformation("Processing workout {WorkoutId}", workoutId);

            var workoutResponse = await _garminConnectHttpClient.GetWorkout(workoutId, cancellationToken);
            _logger.LogInformation("Retrieved Garmin workout details: {workoutId}", workoutId);

            var internalWorkoutStructure = await _converterService.Convert(workoutResponse, cancellationToken);
            var intervalsIcuWorkoutText = new IntervalsIcuWorkoutBuilder()
                .WithSkipDoubledRecovery()
                .Build(internalWorkoutStructure);

            var intervalsIcuEventStructure = IntervalsIcuDescriptionHelper
                .GenerateDescriptionBlock(intervalsIcuWorkoutText);

            var workoutName = GarminConnectWorkoutHelper.GetWorkoutTitle(workoutResponse, ftp);

            var mappingKey = new IntervalsIcuMappingKey
            {
                Date = workout.CalendarDate,
                Title = workoutName
            };

            if (intervalsIcuEventsMapping.TryGetValue(mappingKey, out var existingIntervalsIcuEvent))
            {
                // Check if the existing event is already paired with an activity
                // we will delete items that are not paired with an activity
                intervalsIcuEventsMapping.Remove(mappingKey);

                if (existingIntervalsIcuEvent.Description is { } && HasSameGeneratedContent(intervalsIcuEventStructure, existingIntervalsIcuEvent.Description))
                {
                    _logger.LogInformation("No changes needed for existing event {ExistingIntervalsIcuEventId}", existingIntervalsIcuEvent.Id);
                    continue;
                }
                else
                {
                    _logger.LogInformation("Updating existing event by deletion: {existingIntervalsIcuEventId}", existingIntervalsIcuEvent.Id);
                    await _intervalsIcuHttpClient.DeleteEvent(_athleteId, existingIntervalsIcuEvent.Id, cancellationToken: cancellationToken);
                }
            }

            var (eventType, indoor) = internalWorkoutStructure.Type switch
            {
                Common.Workouts.WorkoutType.Ride => ("Ride", true),
                _ => ("Workout", false)
            };

            var createRequest = new CreateEventRequest
            {
                Category = EventCategory.Workout,
                Description = intervalsIcuEventStructure,
                Name = workoutName,
                Indoor = indoor,
                StartDateLocal = new DateTime(workout.CalendarDate, TimeOnly.MinValue),
                SubType = ActivitySubType.None,
                Tags = [IntervalsIcuEventTagGarminConnect],
                Type = eventType
            };

            _logger.LogInformation("Creating new Intervals.icu event: {CreateRequest}", createRequest);
            await _intervalsIcuHttpClient.CreateEvent(
                _athleteId,
                createRequest,
                cancellationToken: cancellationToken);
        }

        // Delete all events that are not paired with an activity
        foreach (var (key, @event) in intervalsIcuEventsMapping)
        {
            _logger.LogInformation("Deleting unpaired event {EventId} for date {Date} and title {Title}", @event.Id, key.Date, key.Title);
            await _intervalsIcuHttpClient.DeleteEvent(_athleteId, @event.Id, cancellationToken: cancellationToken);
        }

        _logger.LogInformation("Export function completed successfully");

        intervalsIcuEvents = await _intervalsIcuHttpClient.ListEvents(_athleteId,
            new ListEventsQueryParams(firstDay, lastDay), cancellationToken);
        _logger.LogInformation("Retrieved {Count} existing Intervals.icu events", intervalsIcuEvents.Count);

        var intervalsIcuFutureGarminEventsOverview = intervalsIcuEvents
            .Where(x => x.PairedActivityId == null)
            .Aggregate(new StringBuilder(),
                (sb, x) =>
                {
                    var abbreviatedDayName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(x.StartDateLocal.DayOfWeek);
                    sb.Append(abbreviatedDayName);
                    sb.Append('\t');

                    sb.Append($"{x.Type}: {x.Name}");
                    if (x.MovingTime.HasValue)
                    {
                        var timeSpan = TimeSpan.FromSeconds(x.MovingTime.Value);
                        sb.Append(", ");

                        if (timeSpan.Hours > 0)
                        {
                            sb.Append(timeSpan.Hours);
                            sb.Append('h');
                        }

                        if (timeSpan.Minutes > 0)
                        {
                            sb.Append(timeSpan.Minutes);
                            sb.Append('m');
                        }
                    }

                    if (x.IcuTrainingLoad.HasValue)
                    {
                        sb.Append($", Load {x.IcuTrainingLoad.Value}");
                    }

                    return sb.AppendLine();
                },
                sb => sb.ToString());

        return new OkObjectResult(intervalsIcuFutureGarminEventsOverview);
    }

    private static bool HasSameGeneratedContent(string generatedDescription, string existingDescription)
    {
        var existingDescriptionParts = IntervalsIcuDescriptionHelper.ExtractMainBlock(existingDescription);
        var existingIntervalsIcuEventStructure = IntervalsIcuDescriptionHelper.ExtractMainBlock(generatedDescription);

        return existingDescriptionParts.SequenceEqual(existingIntervalsIcuEventStructure);
    }

    private record IntervalsIcuMappingKey
    {
        public required DateOnly Date { get; init; }
        public required string Title { get; init; }
    }

    private static class IntervalsIcuDescriptionHelper
    {
        private const string IntervalsIcuDescriptionAutogeneratedBlockStart = "<!-- BEGIN AUTOGENERATED BLOCK -->";
        private const string IntervalsIcuDescriptionAutogeneratedBlockEnd = "<!-- END AUTOGENERATED BLOCK -->";

        public static string GenerateDescriptionBlock(string intervalsIcuWorkoutStructure)
        {
            var sb = new StringBuilder();
            sb.AppendLine(IntervalsIcuDescriptionAutogeneratedBlockStart);
            sb.AppendLine(intervalsIcuWorkoutStructure);
            sb.AppendLine(IntervalsIcuDescriptionAutogeneratedBlockEnd);

            return sb.ToString();
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
