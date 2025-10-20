using System.Text;
using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions;

public class ZwiftEventsToCompleteRouteAchievementsHttpTriggerFunction
{
    private readonly ZwiftGameInfoService _zwiftGameInfoService;

    public ZwiftEventsToCompleteRouteAchievementsHttpTriggerFunction(
        ZwiftGameInfoService zwiftGameInfoService)
    {
        _zwiftGameInfoService = zwiftGameInfoService;
    }

    [Function(nameof(ZwiftEventsToCompleteRouteAchievementsHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-events-to-complete-achievements")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _ = req;

        var from = DateTimeOffset.UtcNow;
        var to = from.AddDays(7);

        var cyclingRouteToEventMapping = await _zwiftGameInfoService.GetUncompletedRouteToEventsMappingAchievements(
            from, to, cancellationToken);
        var sb = new StringBuilder();

        foreach (var (routeName, events) in cyclingRouteToEventMapping)
        {
            sb.AppendLine(routeName);

            foreach (var @event in events)
            {
                sb.AppendLine($" - {@event.EventStart.ToUniversalTime()}: '{@event.Name}', Id: {@event.Id}");
            }

            sb.AppendLine();
        }

        return new OkObjectResult(sb.ToString());
    }
}
