using FitSyncHub.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions;

public class GarminSyncWeightTimerTriggerFunction
{
    private readonly GarminHealthDataService _garminHealthDataService;
    private readonly ILogger<GarminSyncWeightTimerTriggerFunction> _logger;

    public GarminSyncWeightTimerTriggerFunction(
        GarminHealthDataService garminHealthDataService,
        ILogger<GarminSyncWeightTimerTriggerFunction> logger)
    {
        _garminHealthDataService = garminHealthDataService;
        _logger = logger;
    }

    [Function(nameof(GarminSyncWeightTimerTriggerFunction))]
    public async Task RunMorning(
        // run every day at 4, 5, 6, 7, 13, 19 utc
        [TimerTrigger("0 0 4-7,13,19 * * *")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Trigger executed at: {DateTime}, invocation is due to a missed schedule occurrence: {isPastDue}",
            DateTime.Now,
            timer.IsPastDue);

        await _garminHealthDataService.Sync(cancellationToken);
        _logger.LogInformation("Expected next schedule occurrence, {NextOccurrence}", timer.ScheduleStatus?.Next);
    }
}
