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

    // uncomment when garmin api limits are no longer an issue on production
    //#if !DEBUG
    //    [Function(nameof(GarminSyncWeightTimerTriggerFunction))]
    //#endif
    public async Task RunMorning(
        // run every day at 19 utc to avoid garmin api limits
        [TimerTrigger("0 0 19 * * *")] TimerInfo timer,

        CancellationToken cancellationToken)
    {
#pragma warning disable CA1873 // Avoid potentially expensive logging
        _logger.LogInformation("Trigger executed at: {DateTime}, invocation is due to a missed schedule occurrence: {isPastDue}",
            DateTime.Now,
            timer.IsPastDue);
#pragma warning restore CA1873 // Avoid potentially expensive logging

        await _garminHealthDataService.Sync(cancellationToken);
#pragma warning disable CA1873 // Avoid potentially expensive logging
        _logger.LogInformation("Expected next schedule occurrence, {NextOccurrence}", timer.ScheduleStatus?.Next);
#pragma warning restore CA1873 // Avoid potentially expensive logging
    }
}
