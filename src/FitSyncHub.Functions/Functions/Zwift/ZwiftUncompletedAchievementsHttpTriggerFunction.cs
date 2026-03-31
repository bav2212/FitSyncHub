using System.Text;
using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FitSyncHub.Functions.Functions.Zwift;

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

        sb.AppendLine($"Achievements level (xp): {Math.Round(achievementsState.AchievementLevel, 2)}");
        sb.AppendLine();

        if (achievementsState.GeneralAchievements.Count != 0)
        {
            sb.AppendLine("General achievements:");

            var generalAchievements = achievementsState.GeneralAchievements;

            // Exclude running achievements
            generalAchievements = [.. achievementsState.GeneralAchievements
                .Where(x => !x.Achievement.ImageUrl.Contains("Run", StringComparison.OrdinalIgnoreCase))];

            // Exclude achievements that are not possible to achieve anymore
            generalAchievements = [.. generalAchievements.Where(x => x.Achievement.Id is < 347 or > 471)];

            sb.AppendLine($"Achieved (excluding running and not possible to achieve anymore): {generalAchievements.Count(x => x.IsAchieved)}/{generalAchievements.Count}");

            foreach (var generalAchievementState in generalAchievements)
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
