using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using FitSyncHub.Zwift.Models;
using FitSyncHub.Zwift.Providers.Abstractions;

namespace FitSyncHub.Zwift.Services;

public class ZwiftRouteAchievementResolver
{
    private readonly Dictionary<string, string> _routeNameToAchievementNameExceptions = new(StringComparer.OrdinalIgnoreCase)
    {
        { "2018 WORLDS SHORT LAP", "2018 UCI WORLDS SHORT LAP"},
        { "RICHMOND UCI WORLDS", "2015 UCI WORLDS COURSE"},
        { "WATOPIA HILLY ROUTE", "HILLY ROUTE"},
        { "2019 WORLDS HARROGATE CIRCUIT", "2019 UCI WORLDS HARROGATE CIRCUIT"},
        { "WATOPIA PRETZEL", "THE PRETZEL"},
        { "WATOPIA MOUNTAIN ROUTE", "MOUNTAIN ROUTE"},
        { "WATOPIA MOUNTAIN 8", "MOUNTAIN 8"},
        { "WATOPIA FIGURE 8", "FIGURE 8"},
        { "WATOPIA FIGURE 8 REVERSE", "FIGURE 8 REVERSE"},
        { "WATOPIA FLAT ROUTE", "FLAT ROUTE"},
        { "LONDON PRL HALF", "THE PRL HALF"},
        { "LONDON PRL FULL", "THE PRL FULL"},
        { "CASSE-PATTES", "CASSE PATTES"},
        { "TIRE-BOUCHON", "TIRE BOUCHON"},
        { "HANDFUL OF GRAVEL", "HANDFUL OF GRAVEL (CYCLING)"},
        { "HANDFUL OF GRAVEL RUN", "HANDFUL OF GRAVEL (RUNNING)"},
        { "WATOPIA'S WAISTBAND", "WATOPIAS WAISTBAND"},
        { "RICHMOND 2015 WORLDS REVERSE", "RICHMOND UCI REVERSE"},
        { "CASTLE CRIT RUN", "CASTLE CRIT (RUNNING)"},
        { "TRIPLE TWIST", "TRIPLE TWISTS"},
        { "PEAKY PAVÉ", "PEAKY PAVE"},
    };

    public ZwiftRouteAchievementResolver(List<ZwiftGameInfoAchievement> achievements)
    {
        var isRouteAchievementsLookup = achievements
           .ToLookup(x => x.ImageUrl.EndsWith("RouteComplete.png"));

        RouteAchievements = [.. isRouteAchievementsLookup[true]];
        GeneralAchievements = [.. isRouteAchievementsLookup[false]];
    }

    public List<ZwiftGameInfoAchievement> RouteAchievements { get; }
    public List<ZwiftGameInfoAchievement> GeneralAchievements { get; }

    public Dictionary<ZwiftRouteModel, ZwiftGameInfoAchievement?> MapRoutesToRouteAchievements(
        List<ZwiftDataWorldRoutePair> dataWorldRoutePairs)
    {
        var result = new Dictionary<ZwiftRouteModel, ZwiftGameInfoAchievement?>();

        var routeAchievementsDictionary = RouteAchievements
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var dataWorldRoutePair in dataWorldRoutePairs)
        {
            var route = dataWorldRoutePair.Route;
            var routeName = dataWorldRoutePair.Route.Name;

            if (_routeNameToAchievementNameExceptions.TryGetValue(routeName, out var routeException))
            {
                routeName = routeException;
            }

            if (routeAchievementsDictionary.TryGetValue(routeName, out var value))
            {
                result[route] = value;
                continue;
            }

            result[route] = null;
        }

        return result;
    }

    public Dictionary<ZwiftGameInfoAchievement, ZwiftRouteModel> MapRouteAchievementsToRoutes(
        List<ZwiftDataWorldRoutePair> routes)
    {
        return MapRoutesToRouteAchievements(routes)
            .Where(x => x.Value is not null)
            .ToDictionary(x => x.Value!, x => x.Key);
    }
}
