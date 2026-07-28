using System.Text;
using FitSyncHub.Zwift.Models;
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

            sb.AppendLine($"\tAchieved (excluding running and not possible to achieve anymore): {generalAchievements.Count(x => x.IsAchieved)}/{generalAchievements.Count}");

            var unachievedGeneralAchievements = generalAchievements.Where(x => !x.IsAchieved).ToList();

            if (unachievedGeneralAchievements.Count != 0)
            {
                sb.AppendLine($"\tUnachieved:");
            }

            foreach (var unachievedGeneralAchievement in unachievedGeneralAchievements)
            {
                sb.AppendLine($"\t- {unachievedGeneralAchievement.Achievement.Name}");
            }
            sb.AppendLine();
        }

        if (achievementsState.CyclingRouteAchievementsToRouteMapping.Count != 0)
        {
            sb.AppendLine("Cycling routes:");

            FormatRoutesSummary("Public", x => !x.PublicEventsOnly && !x.ExcludeFromGameDictionary);
            FormatRoutesSummary("EventOnly", x => x.PublicEventsOnly && !x.ExcludeFromGameDictionary);
            FormatRoutesSummary("ExcludeFromGameDictionary", x => x.ExcludeFromGameDictionary);

            void FormatRoutesSummary(string prefix, Func<ZwiftRouteModel, bool> predicate)
            {
                var filteredRoutes = achievementsState.CyclingRouteAchievementsToRouteMapping
                    .Where(x => predicate(x.Value))
                    .ToList();

                if (filteredRoutes.Count == 0)
                {
                    return;
                }

                var nonAchievedItems = filteredRoutes.Where(x => !x.Key.IsAchieved).Select(x => x.Value).ToList();
                var totalCount = filteredRoutes.Count;

                if (nonAchievedItems.Count == 0)
                {
                    return;
                }

                sb.AppendLine(ZwiftUncompletedAchievementsHttpTriggerFunction.FormatRoutesSummary(prefix, nonAchievedItems, totalCount));
            }
        }

        return new OkObjectResult(sb.ToString());
    }

    private static string FormatRoutesSummary(string prefix, List<ZwiftRouteModel> nonAchievedItems, int totalCount)
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
            var totalDistanceKm = Math.Round(route.TotalDistanceInMeters / 1000.0, 1);
            var totalElevation = Math.Round(route.TotalAscentInMeters, 0);

            sb.Append($"\t- {route.Name} ({totalDistanceKm}km, {totalElevation}m)");
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
