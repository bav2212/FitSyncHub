using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using FitSyncHub.Zwift.Models;

namespace FitSyncHub.Zwift.Services.Achievements;

public class ZwiftRouteAchievementResolver
{
    private readonly Dictionary<string, string> _routeNameToAchievementNameExceptions = new(StringComparer.OrdinalIgnoreCase)
    {
        { "RICHMOND UCI WORLDS", "2015 UCI WORLDS COURSE"},
        { "2018 WORLDS SHORT LAP", "2018 UCI WORLDS SHORT LAP"},
        { "2019 WORLDS HARROGATE CIRCUIT", "2019 UCI WORLDS HARROGATE CIRCUIT"},
        { "CASSE-PATTES", "CASSE PATTES"},
        { "CASTLE CRIT RUN", "CASTLE CRIT (RUNNING)"},
        { "HANDFUL OF GRAVEL", "HANDFUL OF GRAVEL (CYCLING)"},
        { "HANDFUL OF GRAVEL RUN", "HANDFUL OF GRAVEL (RUNNING)"},
        { "CLASSIQUE", "LONDON CLASSIQUE"},
        { "CLASSIQUE REVERSE", "LONDON CLASSIQUE REVERSE"},
        { "PEAKY PAVÉ", "PEAKY PAVE"},
        { "RICHMOND 2015 WORLDS REVERSE", "RICHMOND UCI REVERSE"},
        { "TIRE-BOUCHON", "TIRE BOUCHON"},
    };

    public ZwiftRouteAchievementResolver(List<ZwiftGameInfoAchievement> achievements, bool throwOnUntouchedRouteAchievements)
        : this(achievements)
    {
        ThrowOnUntouchedRouteAchievements = throwOnUntouchedRouteAchievements;
    }

    public ZwiftRouteAchievementResolver(List<ZwiftGameInfoAchievement> achievements)
    {
        var isRouteAchievementsLookup = achievements
           .ToLookup(x => x.ImageUrl.EndsWith("RouteComplete.png"));

        RouteAchievements = [.. isRouteAchievementsLookup[true]];
        GeneralAchievements = [.. isRouteAchievementsLookup[false]];
    }

    public List<ZwiftGameInfoAchievement> RouteAchievements { get; }
    public List<ZwiftGameInfoAchievement> GeneralAchievements { get; }
    public bool ThrowOnUntouchedRouteAchievements { get; }

    public Dictionary<ZwiftRouteModel, ZwiftGameInfoAchievement?> MapRoutesToRouteAchievements(
        List<ZwiftRouteModel> routes)
    {
        var result = new Dictionary<ZwiftRouteModel, ZwiftGameInfoAchievement?>();

        var routeAchievementsDictionary = RouteAchievements
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        var touchedRouteAchievementsKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var route in routes)
        {
            var routeName = route.Name;

            if (_routeNameToAchievementNameExceptions.TryGetValue(routeName, out var routeException))
            {
                routeName = routeException;
            }

            if (routeAchievementsDictionary.TryGetValue(routeName, out var value))
            {
                touchedRouteAchievementsKeys.Add(routeName);

                result[route] = value;
                continue;
            }

            result[route] = null;
        }

        var untouchedRouteAchievements = routeAchievementsDictionary
            .Where(x => !touchedRouteAchievementsKeys.Contains(x.Key))
            .ToList();

        if (untouchedRouteAchievements.Count != 0 && ThrowOnUntouchedRouteAchievements)
        {
            var untouchedRouteAchievementsNames = string.Join(", ", untouchedRouteAchievements.Select(x => x.Key));
            throw new InvalidOperationException($"The following route achievements were not matched to any route: {untouchedRouteAchievementsNames}");
        }

        return result;
    }

    public Dictionary<ZwiftGameInfoAchievement, ZwiftRouteModel> MapRouteAchievementsToRoutes(
        List<ZwiftRouteModel> routes)
    {
        return MapRoutesToRouteAchievements(routes)
            .Where(x => x.Value is not null)
            .ToDictionary(x => x.Value!, x => x.Key);
    }
}
