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
        [TimerTrigger("0 * 6-11 * * *")] TimerInfo myTimer,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Morning trigger executed at: {DateTime}", DateTime.Now);

        await _garminHealthDataService.Sync(cancellationToken);
    }

    [Function("GarminSyncWeightRestOfDayTrigger")]
    public async Task RunRestOfDay(
        [TimerTrigger("0 */6 0-5,12-23 * * *")] TimerInfo myTimer,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Rest of day trigger executed at: {DateTime}", DateTime.Now);

        await _garminHealthDataService.Sync(cancellationToken);
    }
}
