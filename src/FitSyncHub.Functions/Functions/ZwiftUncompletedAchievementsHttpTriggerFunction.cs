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
            var orderdRoutes = uncompletedAchievements.CyclingRouteAchievementsToRouteMapping.Values
                .OrderBy(x => x.PublicEventsOnly)
                .ThenBy(x => x.PublishedOn)
                .ToList();

            foreach (var route in orderdRoutes)
            {
                var totalDistanceKm = Math.Round(route.TotalDistanceInMeters / 1000.0, 1);
                var totalElevation = Math.Round(route.TotalAscentInMeters, 0);

                sb.Append($"- {route.Name} ({totalDistanceKm}km, {totalElevation}m)");
                if (route.PublicEventsOnly)
                {
                    sb.Append(", events only");
                }
                if (route.PublishedOn.HasValue && route.PublishedOn > DateOnly.FromDateTime(DateTime.Today))
                {
                    sb.Append($", will published on {route.PublishedOn:yyyy-MM-dd}");
                }
                sb.AppendLine();

            }
            sb.AppendLine();
        }

        return new OkObjectResult(sb.ToString());
    }
}
