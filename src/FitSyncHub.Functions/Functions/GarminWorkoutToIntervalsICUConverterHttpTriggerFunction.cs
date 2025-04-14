using System.Text;
using FitSyncHub.Common.Applications.IntervalsIcu;
using FitSyncHub.GarminConnect.Converters;
using FitSyncHub.GarminConnect.HttpClients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions;

public class GarminWorkoutToIntervalsICUConverterHttpTriggerFunction
{
    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly ILogger<GarminWorkoutToIntervalsICUConverterHttpTriggerFunction> _logger;

    public GarminWorkoutToIntervalsICUConverterHttpTriggerFunction(
        GarminConnectHttpClient garminConnectHttpClient,
        ILogger<GarminWorkoutToIntervalsICUConverterHttpTriggerFunction> logger)
    {
        _garminConnectHttpClient = garminConnectHttpClient;
        _logger = logger;
    }

    [Function(nameof(GarminWorkoutToIntervalsICUConverterHttpTriggerFunction))]
    public async Task<ActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "convert-garmin-workout-to-intervals")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        string? dateQueryParameter = req.Query["date"];
        if (!DateOnly.TryParse(dateQueryParameter, out var date))
        {
            date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        }

        var ftp = await _garminConnectHttpClient.GetCyclingFtp(cancellationToken: cancellationToken);

        var trainingPlanId = await _garminConnectHttpClient.GetActiveTrainingPlanId(cancellationToken);
        var trainingPlan = await _garminConnectHttpClient.GetTrainingPlan(trainingPlanId, cancellationToken);

        var taskListForDate = trainingPlan.TaskList
            .Where(x => x.CalendarDate == date && x.TaskWorkout.SportType != null)
            .ToList();

        if (taskListForDate.Count == 0)
        {
            return new BadRequestObjectResult("No activities for specified date");
        }

        var cyclingTaskListForDate = taskListForDate
            .Where(x => x.TaskWorkout.SportType.SportTypeKey == "cycling")
            .ToList();

        if (cyclingTaskListForDate.Count == 0)
        {
            return new BadRequestObjectResult("No cycling activities for specified date");
        }

        var result = new StringBuilder();

        foreach (var cyclingTask in cyclingTaskListForDate)
        {
            var workoutId = cyclingTask.TaskWorkout.WorkoutUuid;

            var workoutResponse = await _garminConnectHttpClient.GetWorkout(workoutId, cancellationToken);

            var icuGroups = GarminConnectToIntervalsIcuWorkoutConverter
                .ConvertGarminWorkoutToIntervalsIcuWorkoutGroups(workoutResponse, ftp);

            var intervalsIcuWorkoutLines = IntervalsIcuConverter
                .ConvertToIntervalsIcuFormat(icuGroups);

            result.AppendLine($"{workoutResponse.WorkoutName}");
            result.AppendLine($"{workoutResponse.Description}\n");
            foreach (var item in intervalsIcuWorkoutLines)
            {
                result.AppendLine(item);
            }
            result.AppendLine();
        }

        return new OkObjectResult(result.ToString());
    }
}
