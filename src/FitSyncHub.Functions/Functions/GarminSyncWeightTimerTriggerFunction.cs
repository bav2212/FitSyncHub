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


    [Function("GarminSyncWeightMorningTrigger")]
    public async Task RunMorning(
        // Run every hour from 4 to 7 utc
        [TimerTrigger("0 * 4-7 * * *")] TimerInfo myTimer,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Morning trigger executed at: {DateTime}", DateTime.Now);

        await _garminHealthDataService.Sync(cancellationToken);
    }

    [Function("GarminSyncWeightRestOfDayTrigger")]
    public async Task RunRestOfDay(
        // Run every 6 hours from 0 to 3 and 8 to 23 utc
        [TimerTrigger("0 */6 0-3,8-23 * * *")] TimerInfo myTimer,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Rest of day trigger executed at: {DateTime}", DateTime.Now);

        await _garminHealthDataService.Sync(cancellationToken);
    }
}
