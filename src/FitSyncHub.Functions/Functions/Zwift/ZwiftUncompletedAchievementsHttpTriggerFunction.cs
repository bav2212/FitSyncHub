using System.Text;
using FitSyncHub.Zwift.Services;
using FitSyncHub.Zwift.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FitSyncHub.Functions.Functions.Zwift;

public class ZwiftUncompletedAchievementsHttpTriggerFunction
{
    private readonly ZwiftAchievementsService _zwiftAchievementsService;

    public ZwiftUncompletedAchievementsHttpTriggerFunction(
        ZwiftAchievementsService zwiftAchievementsService)
    {
        _zwiftAchievementsService = zwiftAchievementsService;
    }

    [Function(nameof(ZwiftUncompletedAchievementsHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-uncompleted-achievements")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _ = req;

        var achievementsState = await _zwiftAchievementsService.GetAchievementProgress(cancellationToken);
        var sb = new StringBuilder();

        sb.AppendLine($"Achievements level (xp): {achievementsState.AchievementLevel:0.00}");
        sb.AppendLine();

        if (achievementsState.GeneralAchievements.Count != 0)
        {
            sb.AppendLine("General achievements:");

            var generalAchievements = achievementsState.GeneralAchievements;

            sb.AppendLine($"\tAchieved (excluding running and not possible to achieve anymore): {generalAchievements.Count(x => x.IsAchieved)}/{generalAchievements.Count}");

            var unachievedGeneralAchievements = generalAchievements.Where(x => !x.IsAchieved).ToList();

            if (unachievedGeneralAchievements.Count != 0)
            {
                sb.AppendLine($"\tUnachieved:");
            }

            foreach (var unachievedGeneralAchievement in unachievedGeneralAchievements)
            {
                sb.AppendLine($"\t- {unachievedGeneralAchievement.Name}");
            }
            sb.AppendLine();
        }

        if (achievementsState.RouteAchievements.Count != 0)
        {
            sb.AppendLine("Cycling routes:");

            foreach (var item in achievementsState.RouteAchievements)
            {
                var type = item.Type;
                var filteredRoutes = item.Routes;

                if (filteredRoutes.Count == 0)
                {
                    continue;
                }

                var nonAchievedItems = filteredRoutes.Where(x => !x.Key.IsAchieved).Select(x => x.Value).ToList();
                var totalCount = filteredRoutes.Count;

                if (nonAchievedItems.Count == 0)
                {
                    continue;
                }

                sb.AppendLine(FormatRoutesSummary(type, nonAchievedItems, totalCount));

            }
        }

        return new OkObjectResult(sb.ToString());
    }

    private static string FormatRoutesSummary(string prefix, List<ZwiftRouteAchievementInfo> nonAchievedItems, int totalCount)
    {
        StringBuilder sb = new();

        sb.AppendLine($"{prefix} routes:");

        sb.AppendLine($"\tAchieved: {totalCount - nonAchievedItems.Count}/{totalCount}");
        if (nonAchievedItems.Count != 0)
        {
            sb.AppendLine($"\tUnachieved:");
        }

        foreach (var route in nonAchievedItems.OrderBy(x => x.PublishedOn))
        {
            sb.Append($"\t- {route.Name}");

            var totalDistanceKm = Math.Round(route.TotalDistanceInMeters / 1000.0, 1);
            var totalElevation = Math.Round(route.TotalAscentInMeters, 0);
            sb.Append($" ({totalDistanceKm}km, {totalElevation}m)");

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

        return sb.ToString();
    }
}
