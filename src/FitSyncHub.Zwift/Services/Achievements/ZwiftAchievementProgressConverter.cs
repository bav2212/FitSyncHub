using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using FitSyncHub.Zwift.Models;
using FitSyncHub.Zwift.Protobuf;
using FitSyncHub.Zwift.Services.Models;

namespace FitSyncHub.Zwift.Services.Achievements;

public sealed class ZwiftAchievementProgressConverter
{
    private readonly PlayerProfile _profileMe;
    private readonly ZwiftRouteAchievementResolver _zwiftAchievementsResolver;
    private readonly List<ZwiftRouteModel> _routes;
    private readonly HashSet<int> _userAchievements;
    private readonly LoadPlayerRouteCompletionAchievementsResponse _routeCompletionAchievements;

    public ZwiftAchievementProgressConverter(
        PlayerProfile profileMe,
        ZwiftRouteAchievementResolver zwiftAchievementsResolver,
        List<ZwiftRouteModel> routes,
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
            GeneralAchievements = [.. generalAchievements.Select(ConvertToAchievementStatus)]
        };
    }

    private IEnumerable<ZwiftRouteAchievementGroup> ConvertRouteAchievements()
    {
        var mappedRouteAchievementsToRoutes = _zwiftAchievementsResolver.MapRouteAchievementsToRoutes(_routes);

        var routeAchievementsToRouteMapping = mappedRouteAchievementsToRoutes
            .Where(x => x.Value.Sports.Contains(ZwiftGameInfoSport.Cycling))
            .ToDictionary(
                x => ConvertToAchievementStatus(x.Key),
                x => x.Value
            );

        yield return CreateCoreRouteAchievementModel(
            routeAchievementsToRouteMapping, "Public", x => !x.PublicEventsOnly && !x.ExcludeFromGameDictionary);
        yield return CreateCoreRouteAchievementModel(
            routeAchievementsToRouteMapping, "EventOnly", x => x.PublicEventsOnly && !x.ExcludeFromGameDictionary);
        yield return CreateCoreRouteAchievementModel(
            routeAchievementsToRouteMapping, "ExcludeFromGameDictionary", x => x.ExcludeFromGameDictionary);

        yield return new ZwiftRouteAchievementGroup
        {
            Type = "Video",
            Routes = _routeCompletionAchievements.Achievements
                .Where(x => x.Sport == AchievementSport.Cycling)
                .ToDictionary(
                    x => new ZwiftAchievementStatus { Name = x.AnalyticsName, IsAchieved = x.Unlocked },
                    x => new ZwiftRouteAchievementInfo
                    {
                        Name = x.DisplayTitle,
                        PublicEventsOnly = false,
                        PublishedOn = null,
                        // temp
                        TotalAscentInMeters = null,
                        // temp
                        TotalDistanceInMeters = null,
                    })
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
            Routes = filteredRoutes.ToDictionary(
                x => x.Key,
                x => new ZwiftRouteAchievementInfo
                {
                    Name = x.Value.Name,
                    PublicEventsOnly = x.Value.PublicEventsOnly,
                    PublishedOn = x.Value.PublishedOn,
                    TotalAscentInMeters = x.Value.TotalAscentInMeters,
                    TotalDistanceInMeters = x.Value.TotalDistanceInMeters,
                }
            )
        };
    }

    private ZwiftAchievementStatus ConvertToAchievementStatus(ZwiftGameInfoAchievement achievement)
    {
        return new ZwiftAchievementStatus
        {
            Name = achievement.Name,
            IsAchieved = _userAchievements.Contains(achievement.Id)
        };
    }
}
