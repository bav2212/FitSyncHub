using System.Text;
using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions;

public class ZwiftMissingRouteAchievementsHttpTriggerFunction
{
    private readonly ZwiftGameInfoService _zwiftGameInfoService;

    public ZwiftMissingRouteAchievementsHttpTriggerFunction(
        ZwiftGameInfoService zwiftGameInfoService)
    {
        _zwiftGameInfoService = zwiftGameInfoService;
    }

    [Function(nameof(ZwiftMissingRouteAchievementsHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-route-achievements")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _ = req;


        var missingAchievements = await _zwiftGameInfoService.GetMissingCyclingRouteAchievements(cancellationToken);
        var sb = new StringBuilder();

        foreach (var missingAchievement in missingAchievements)
        {
            sb.AppendLine(missingAchievement.Name);
        }

        return new OkObjectResult(sb.ToString());
    }
}
