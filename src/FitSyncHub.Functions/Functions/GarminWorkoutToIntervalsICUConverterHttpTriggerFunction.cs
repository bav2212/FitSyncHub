using System.Text;
using FitSyncHub.Common.Applications.IntervalsIcu;
using FitSyncHub.GarminConnect.Converters;
using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.GarminConnect.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions;

public class GarminWorkoutToIntervalsICUConverterHttpTriggerFunction
{
    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly GarminConnectToIntervalsIcuWorkoutConvertionService _convertionService;
    private readonly ILogger<GarminWorkoutToIntervalsICUConverterHttpTriggerFunction> _logger;

    public GarminWorkoutToIntervalsICUConverterHttpTriggerFunction(
        GarminConnectHttpClient garminConnectHttpClient,
        GarminConnectToIntervalsIcuWorkoutConvertionService convertionService,
        ILogger<GarminWorkoutToIntervalsICUConverterHttpTriggerFunction> logger)
    {
        _garminConnectHttpClient = garminConnectHttpClient;
        _convertionService = convertionService;
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

        var result = new StringBuilder();

        foreach (var workout in taskListForDate)
        {
            var workoutId = workout.TaskWorkout.WorkoutUuid;

            var workoutResponse = await _garminConnectHttpClient.GetWorkout(workoutId, cancellationToken);

            result.AppendLine($"{GarminConnectWorkoutHelper.GetWorkoutTitle(workoutResponse, ftp)}");

            var icuGroups = await _convertionService.Convert(workoutResponse, cancellationToken);
            var intervalsIcuWorkoutDescription = IntervalsIcuConverter.ConvertToIntervalsIcuFormat(icuGroups);

            result.AppendLine(intervalsIcuWorkoutDescription);

            result.AppendLine();
            result.AppendLine();
        }

        return new OkObjectResult(result.ToString());
    }
}
