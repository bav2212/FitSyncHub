using System.Text;
using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions;

public class ZwiftUncompletedRouteAchievementsHttpTriggerFunction
{
    private readonly ZwiftGameInfoService _zwiftGameInfoService;

    public ZwiftUncompletedRouteAchievementsHttpTriggerFunction(
        ZwiftGameInfoService zwiftGameInfoService)
    {
        _zwiftGameInfoService = zwiftGameInfoService;
    }

    [Function(nameof(ZwiftUncompletedRouteAchievementsHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-route-achievements")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _ = req;

        var uncompletedAchievements = await _zwiftGameInfoService.GetUncompletedCyclingRouteAchievements(cancellationToken);
        var sb = new StringBuilder();

        foreach (var uncompletedAchievement in uncompletedAchievements)
        {
            sb.AppendLine(uncompletedAchievement.Name);
        }

        return new OkObjectResult(sb.ToString());
    }
}
