using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Models.Requests.Events;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Events;
using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using FitSyncHub.Zwift.Providers.Abstractions;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Zwift.Services;

public class ZwiftGameInfoService
{
    private readonly ZwiftHttpClient _zwiftHttpClient;
    private readonly IZwiftRoutesProvider _zwiftRoutesProvider;
    private readonly ILogger<ZwiftGameInfoService> _logger;

    // from https://github.com/zoffline/zwift-offline/blob/master/scripts/get_game_dictionary.py
    private readonly Dictionary<string, string> _routeExceptions = new(StringComparer.OrdinalIgnoreCase)
    {
        {"2018 UCI WORLDS SHORT LAP", "2018 WORLDS SHORT LAP"},
        {"2015 UCI WORLDS COURSE", "RICHMOND UCI WORLDS"},
        {"HILLY ROUTE", "WATOPIA HILLY ROUTE"},
        {"2019 UCI WORLDS HARROGATE CIRCUIT", "2019 WORLDS HARROGATE CIRCUIT"},
        {"THE PRETZEL", "THE LONDON PRETZEL"},
        {"MOUNTAIN ROUTE", "WATOPIA MOUNTAIN ROUTE"},
        {"MOUNTAIN 8", "WATOPIA MOUNTAIN 8"},
        {"FIGURE 8", "WATOPIA FIGURE 8"},
        {"FIGURE 8 REVERSE", "WATOPIA FIGURE 8 REVERSE"},
        {"FLAT ROUTE", "WATOPIA FLAT ROUTE"},
        {"THE PRL HALF", "LONDON PRL HALF"},
        {"THE PRL FULL", "LONDON PRL FULL"},
        {"CASSE PATTES", "CASSE-PATTES"},
        {"TIRE BOUCHON", "TIRE-BOUCHON"},
        {"HANDFUL OF GRAVEL (CYCLING)", "HANDFUL OF GRAVEL"},
        {"HANDFUL OF GRAVEL (RUNNING)", "HANDFUL OF GRAVEL RUN"},
        {"WATOPIAS WAISTBAND", "WATOPIA'S WAISTBAND"},
        {"RICHMOND UCI REVERSE", "RICHMOND 2015 WORLDS REVERSE"},
        {"CASTLE CRIT (RUNNING)", "CASTLE CRIT RUN"},
        {"TRIPLE TWISTS", "TRIPLE TWIST"},
    };

    public ZwiftGameInfoService(
        ZwiftHttpClient zwiftHttpClient,
        IZwiftRoutesProvider zwiftRoutesProvider,
        ILogger<ZwiftGameInfoService> logger)
    {
        _zwiftHttpClient = zwiftHttpClient;
        _zwiftRoutesProvider = zwiftRoutesProvider;
        _logger = logger;
    }

    public async Task<Dictionary<ZwiftGameInfoRoute, List<ZwiftEventResponse>>> GetUncompletedRouteToEventsMappingAchievements(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var uncompletedCyclingRouteAchievementsWithMappingToRoute = await GetMappedUncompletedAchievements(cancellationToken);

        var events = await _zwiftHttpClient.GetEventFeedFullRangeBuggy(new ZwiftEventFeedRequest
        {
            From = from,
            To = to,
        }, cancellationToken);

        var routesWithUncompletedAchievementsDictionary = uncompletedCyclingRouteAchievementsWithMappingToRoute
            .CyclingRouteAchievementsToRouteMapping
            .Values
            .ToDictionary(x => x.Id, x => x);

        var eventsToRouteMapping = events
            .Join(routesWithUncompletedAchievementsDictionary,
                x => x.RouteId,
                x => x.Key,
                (@event, route) => new { Event = @event, Route = route.Value })
            .ToList();

        return eventsToRouteMapping
            .GroupBy(x => x.Route)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Event).ToList());
    }

    public async Task<MappedUncompletedAchievementsModel> GetMappedUncompletedAchievements(
        CancellationToken cancellationToken)
    {
        var gameInfo = await _zwiftHttpClient.GetGameInfo(cancellationToken);
        var isRouteAchievementsLookup = gameInfo.Achievements
            .ToLookup(x => x.ImageUrl.EndsWith("RouteComplete.png"));

        var routeAchievements = isRouteAchievementsLookup[true];
        var generalAchievements = isRouteAchievementsLookup[false];

        var routes = await _zwiftRoutesProvider.GetRoutesInfo(cancellationToken);
        var userAchievements = (await _zwiftHttpClient.GetAchievements(cancellationToken)).ToHashSet();

        var mappedRouteAchievementsToRoutes = MapRouteAchievementsToRoutes([.. routeAchievements], routes);

        return new MappedUncompletedAchievementsModel
        {
            CyclingRouteAchievementsToRouteMapping = GetRouteAchievementsToRouteMappingForSport(ZwiftGameInfoSport.Cycling),
            RunningRouteAchievementsToRouteMapping = GetRouteAchievementsToRouteMappingForSport(ZwiftGameInfoSport.Running),
            GeneralAchievements = [.. generalAchievements.Where(x => !userAchievements.Contains(x.Id))]
        };

        Dictionary<ZwiftGameInfoAchievement, ZwiftGameInfoRoute> GetRouteAchievementsToRouteMappingForSport(ZwiftGameInfoSport sport)
        {
            return mappedRouteAchievementsToRoutes
                .Where(x => x.Value.Sports.Contains(sport) && !userAchievements.Contains(x.Key.Id))
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }

    private Dictionary<ZwiftGameInfoAchievement, ZwiftGameInfoRoute> MapRouteAchievementsToRoutes(
        List<ZwiftGameInfoAchievement> routeAchievements,
        List<ZwiftDataWorldRoutePair> routes)
    {
        var result = new Dictionary<ZwiftGameInfoAchievement, ZwiftGameInfoRoute>();

        var routesDictionary = routes
            .Select(x => x.Route)
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var routeAchievement in routeAchievements)
        {
            if (routesDictionary.TryGetValue(routeAchievement.Name, out var value))
            {
                result[routeAchievement] = value;
                continue;
            }

            if (_routeExceptions.TryGetValue(routeAchievement.Name, out var routeException))
            {
                result[routeAchievement] = routesDictionary[routeException];
                continue;
            }

            _logger.LogWarning("Cannot map achievement '{Name}' to any route.", routeAchievement.Name);
        }

        return result;
    }
}

public record MappedUncompletedAchievementsModel
{
    public required Dictionary<ZwiftGameInfoAchievement, ZwiftGameInfoRoute> CyclingRouteAchievementsToRouteMapping { get; init; }
    public required Dictionary<ZwiftGameInfoAchievement, ZwiftGameInfoRoute> RunningRouteAchievementsToRouteMapping { get; init; }
    public required List<ZwiftGameInfoAchievement> GeneralAchievements { get; init; }
}
