using System.Text;
using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions;

public class ZwiftMissingAchievementsHttpTriggerFunction
{
    private readonly ZwiftAchievementsService _zwiftAchievementsService;

    public ZwiftMissingAchievementsHttpTriggerFunction(
        ZwiftAchievementsService zwiftAchievementsService)
    {
        _zwiftAchievementsService = zwiftAchievementsService;
    }

    [Function(nameof(ZwiftMissingAchievementsHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-achievements")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _ = req;

        var missingAchievements = await _zwiftAchievementsService.GetMissingAchievements(cancellationToken);
        var sb = new StringBuilder();

        foreach (var missingAchievement in missingAchievements)
        {
            sb.AppendLine(missingAchievement.Name);
        }

        return new OkObjectResult(sb.ToString());
    }
}
