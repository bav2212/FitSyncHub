using System.Text;
using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions;

public class ZwiftUncompletedAchievementsHttpTriggerFunction
{
    private readonly ZwiftGameInfoService _zwiftGameInfoService;

    public ZwiftUncompletedAchievementsHttpTriggerFunction(
        ZwiftGameInfoService zwiftGameInfoService)
    {
        _zwiftGameInfoService = zwiftGameInfoService;
    }

    [Function(nameof(ZwiftUncompletedAchievementsHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-uncompleted-achievements")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _ = req;

        var uncompletedAchievements = await _zwiftGameInfoService.GetMappedUncompletedAchievements(cancellationToken);
        var sb = new StringBuilder();

        if (uncompletedAchievements.GeneralAchievements.Count != 0)
        {
            sb.AppendLine("General achievements:");
            foreach (var uncompletedGeneralAchievement in uncompletedAchievements.GeneralAchievements)
            {
                if (uncompletedGeneralAchievement.ImageUrl.Contains("Run", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                sb.AppendLine($"- {uncompletedGeneralAchievement.Name}");
            }
            sb.AppendLine();
        }

        if (uncompletedAchievements.CyclingRouteAchievementsToRouteMapping.Count != 0)
        {
            sb.AppendLine("Cycling routes:");
            foreach (var (_, route) in uncompletedAchievements.CyclingRouteAchievementsToRouteMapping)
            {
                sb.AppendLine($"- {route.Name}");
            }
            sb.AppendLine();
        }

        return new OkObjectResult(sb.ToString());
    }
}
