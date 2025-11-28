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

        var achievementsState = await _zwiftGameInfoService.GetAchievementsState(cancellationToken);
        var sb = new StringBuilder();

        if (achievementsState.GeneralAchievements.Count != 0)
        {
            sb.AppendLine("General achievements:");

            var nonRunningGeneralAchievements = achievementsState.GeneralAchievements
                .Where(x => !x.Achievement.ImageUrl.Contains("Run", StringComparison.OrdinalIgnoreCase))
                .ToList();

            sb.AppendLine($"Achieved (excluding running): {nonRunningGeneralAchievements.Count(x => x.IsAchieved)}/{nonRunningGeneralAchievements.Count}");

            foreach (var generalAchievementState in nonRunningGeneralAchievements)
            {
                if (generalAchievementState.IsAchieved)
                {
                    continue;
                }

                sb.AppendLine($"- {generalAchievementState.Achievement.Name}");
            }
            sb.AppendLine();
        }

        if (achievementsState.CyclingRouteAchievementsToRouteMapping.Count != 0)
        {
            sb.AppendLine("Cycling routes:");

            var statsLookup = achievementsState.CyclingRouteAchievementsToRouteMapping
                .GroupBy(x => new
                {
                    x.Value.PublicEventsOnly
                })
                .ToDictionary(x => x.Key.PublicEventsOnly, g => new
                {
                    IsAchievedCount = g.Count(x => x.Key.IsAchieved),
                    Count = g.Count()
                });

            sb.AppendLine($"Achieved (public routes): {statsLookup[false].IsAchievedCount}/{statsLookup[false].Count}");
            sb.AppendLine($"Achieved (eventOnly routes): {statsLookup[true].IsAchievedCount}/{statsLookup[true].Count}");

            var uncompletedRoutesOrdered = achievementsState.CyclingRouteAchievementsToRouteMapping
                .Where(x => !x.Key.IsAchieved)
                .Select(x => x.Value)
                .OrderBy(x => x.PublicEventsOnly)
                .ThenBy(x => x.PublishedOn)
                .ToList();

            foreach (var route in uncompletedRoutesOrdered)
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
