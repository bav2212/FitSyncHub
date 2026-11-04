using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using FitSyncHub.Zwift.Models;
using FitSyncHub.Zwift.Protobuf;
using FitSyncHub.Zwift.Providers.Abstractions;
using FitSyncHub.Zwift.Services.Models;

namespace FitSyncHub.Zwift.Services.Achievements;

public sealed class ZwiftAchievementProgressConverter
{
    private readonly PlayerProfile _profileMe;
    private readonly ZwiftRouteAchievementResolver _zwiftAchievementsResolver;
    private readonly ZwiftParsedRoutesResponse _routes;
    private readonly HashSet<int> _userAchievements;
    private readonly LoadPlayerRouteCompletionAchievementsResponse _routeCompletionAchievements;

    public ZwiftAchievementProgressConverter(
        PlayerProfile profileMe,
        ZwiftRouteAchievementResolver zwiftAchievementsResolver,
        ZwiftParsedRoutesResponse routes,
        HashSet<int> userAchievements,
        LoadPlayerRouteCompletionAchievementsResponse routeCompletionAchievements)
    {
        _profileMe = profileMe;
        _zwiftAchievementsResolver = zwiftAchievementsResolver;
        _routes = routes;
        _userAchievements = userAchievements;
        _routeCompletionAchievements = routeCompletionAchievements;
    }

    public ZwiftAchievementProgress Convert()
    {
        var generalAchievements = _zwiftAchievementsResolver.GeneralAchievements;

        // Exclude running achievements
        generalAchievements = [.. generalAchievements
                .Where(x => !x.ImageUrl.Contains("Run", StringComparison.OrdinalIgnoreCase))];

        // Exclude achievements that are not possible to achieve anymore
        generalAchievements = [.. generalAchievements.Where(x => x.Id is < 347 or > 471)];

        return new ZwiftAchievementProgress
        {
            AchievementLevel = _profileMe.AchievementLevel / 100.0,
            RouteAchievements = [.. ConvertRouteAchievements()],
            GeneralAchievements = [.. generalAchievements.Select(ConvertAchievement)]
        };
    }

    private IEnumerable<ZwiftRouteAchievementGroup> ConvertRouteAchievements()
    {
        var mappedRouteAchievementsToRoutes = _zwiftAchievementsResolver.MapRouteAchievementsToRoutes(_routes.RegularRoutes);

        var routeAchievementsToRouteMapping = mappedRouteAchievementsToRoutes
            .Where(x => x.Value.Sports.Contains(ZwiftGameInfoSport.Cycling))
            .ToDictionary(
                x => ConvertAchievement(x.Key),
                x => x.Value
            );

        yield return CreateCoreRouteAchievementModel(
            routeAchievementsToRouteMapping, "Public", x => !x.PublicEventsOnly && !x.ExcludeFromGameDictionary);
        yield return CreateCoreRouteAchievementModel(
            routeAchievementsToRouteMapping, "EventOnly", x => x.PublicEventsOnly && !x.ExcludeFromGameDictionary);
        yield return CreateCoreRouteAchievementModel(
            routeAchievementsToRouteMapping, "ExcludeFromGameDictionary", x => x.ExcludeFromGameDictionary);

        var mochiRoutesById = _routes.MochiRoutes.ToDictionary(x => x.Id, x => x);
        var videAchievementGroupRoutes = _routeCompletionAchievements.Achievements
            .Where(x => x.Sport == AchievementSport.Cycling)
            .Select(achievement => new
            {
                Achievement = new ZwiftAchievementStatus { Name = achievement.AnalyticsName, IsAchieved = achievement.Unlocked },
                // temp hack to have some data for the video routes, since we don't have the route data in the game info API
                Route = mochiRoutesById.TryGetValue((long)achievement.RouteHash, out var route)
                        ? ConvertRoute(route) with
                        {
                            // temp to have pretty name
                            Name = achievement.DisplayTitle
                        }
                        : new()
                        {
                            Name = achievement.DisplayTitle,
                            PublicEventsOnly = false,
                            PublishedOn = null,
                            TotalAscentInMeters = 0,
                            TotalDistanceInMeters = 0
                        }
            }).ToDictionary(x => x.Achievement, x => x.Route);

        yield return new ZwiftRouteAchievementGroup
        {
            Type = "Video",
            Routes = videAchievementGroupRoutes
        };
    }

    private static ZwiftRouteAchievementGroup CreateCoreRouteAchievementModel(
        Dictionary<ZwiftAchievementStatus, ZwiftRouteModel> routeAchievementsToRouteMapping,
        string type,
        Func<ZwiftRouteModel, bool> predicate)
    {
        var filteredRoutes = routeAchievementsToRouteMapping
            .Where(x => predicate(x.Value))
            .ToList();

        if (filteredRoutes.Count == 0)
        {
            return new ZwiftRouteAchievementGroup
            {
                Type = type,
                Routes = [],
            };
        }

        var nonAchievedItems = filteredRoutes.Where(x => !x.Key.IsAchieved).Select(x => x.Value).ToList();
        var totalCount = filteredRoutes.Count;

        return new ZwiftRouteAchievementGroup
        {
            Type = type,
            Routes = filteredRoutes.ToDictionary(x => x.Key, x => ConvertRoute(x.Value))
        };
    }

    private static ZwiftRouteAchievementInfo ConvertRoute(ZwiftRouteModel route)
    {
        return new ZwiftRouteAchievementInfo
        {
            Name = route.Name,
            PublicEventsOnly = route.PublicEventsOnly,
            PublishedOn = route.PublishedOn,
            TotalAscentInMeters = route.TotalAscentInMeters,
            TotalDistanceInMeters = route.TotalDistanceInMeters,
        };
    }

    private ZwiftAchievementStatus ConvertAchievement(ZwiftGameInfoAchievement achievement)
    {
        return new ZwiftAchievementStatus
        {
            Name = achievement.Name,
            IsAchieved = _userAchievements.Contains(achievement.Id)
        };
    }
}
