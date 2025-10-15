using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Zwift.Services;

public class ZwiftGameInfoService
{
    private readonly ZwiftHttpClient _zwiftHttpClient;
    private readonly ILogger<ZwiftGameInfoService> _logger;

    private readonly Dictionary<string, string> _mapNameMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        // check maybe it's wrong
        {"", "Climb Portal"},
        {"NEWYORK", "New York"},
        {"WATOPIA", "Watopia"},
        {"SCOTLAND", "Scotland"},
        {"RICHMOND", "Richmond"},
        {"PARIS", "Paris"},
        {"MAKURIISLANDS", "Makuri Islands"},
        {"LONDON", "London"},
        {"YORKSHIRE", "Yorkshire"},
        {"FRANCE", "France"},
        {"INNSBRUCK", "Innsbruck"},
        {"CRITCITY", "Crit City"},
        {"BOLOGNATT", "Bologna"},
        {"GRAVEL MOUNTAIN", "Gravel Mountain"},
    };


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
        ILogger<ZwiftGameInfoService> logger)
    {
        _zwiftHttpClient = zwiftHttpClient;
        _logger = logger;
    }

    public async Task<List<ZwiftDataRoutesInfoModel>> GetRoutesInfo(CancellationToken cancellationToken)
    {
        var gameInfo = await _zwiftHttpClient.GetGameInfo(cancellationToken);

        var worldRoutePairs = gameInfo.Maps.SelectMany(x => x.Routes.Select(y => new
        {
            World = x,
            Route = y,
        })).ToList();

        var result = worldRoutePairs
            .Where(x => x.Route.Sports.Contains(ZwiftGameInfoSport.Cycling))
            .Select(pair =>
            {
                var route = pair.Route;
                var world = pair.World;

                var restictions = new List<string>();

                if (route.Sports.Count == 1 && route.Sports.Contains(ZwiftGameInfoSport.Running))
                {
                    restictions.Add("Run Only");
                }

                if (route.PublicEventsOnly)
                {
                    restictions.Add("Event Only");
                }

                if (route.LevelLocked > 1)
                {
                    restictions.Add($"Level {route.LevelLocked}+");

                }

                return new ZwiftDataRoutesInfoModel
                {
                    Name = route.Name,
                    WorldName = _mapNameMapping[world.Name],
                    Distance = Math.Round(route.DistanceInMeters / 1000, 1),
                    ElevationGain = Math.Round(route.AscentInMeters),
                    LeadIn = Math.Round(route.LeadinDistanceInMeters / 1000, 1),
                    LeadInElevationGain = Math.Round(route.LeadinAscentInMeters),
                    Restrictions = restictions.Count != 0 ? string.Join(", ", restictions) : null,
                };
            });

        return [..result
            .OrderBy(x => x.WorldName)
            .ThenBy(x => x.Name)];
    }


    public async Task<List<ZwiftGameInfoAchievement>> GetMissingCyclingRouteAchievements(CancellationToken cancellationToken)
    {
        var gameInfo = await _zwiftHttpClient.GetGameInfo(cancellationToken);
        var routeAchievements = gameInfo.Achievements
            .Where(x => x.ImageUrl.EndsWith("RouteComplete.png"))
            .ToList();

        var mappedRouteAchievementsToRoutes = MapRouteAchievementsToRoutes(
            gameInfo.Maps, routeAchievements);
        var cyclingRouteAchievements = mappedRouteAchievementsToRoutes
            .Where(x => x.Value.Sports.Contains(ZwiftGameInfoSport.Cycling))
            .Select(x => x.Key)
            .ToList();

        var userAchievements = (await _zwiftHttpClient.GetAchievements(cancellationToken)).ToHashSet();

        return [.. cyclingRouteAchievements.Where(x => !userAchievements.Contains(x.Id))];
    }

    private Dictionary<ZwiftGameInfoAchievement, ZwiftGameInfoRoute> MapRouteAchievementsToRoutes(
        List<ZwiftGameInfoMap> maps,
        List<ZwiftGameInfoAchievement> routeAchievements)
    {
        var result = new Dictionary<ZwiftGameInfoAchievement, ZwiftGameInfoRoute>();

        var routesDictionary = maps
            .SelectMany(x => x.Routes)
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

public record ZwiftDataRoutesInfoModel
{
    public required string Name { get; init; }
    public required string WorldName { get; init; }
    public required double Distance { get; init; }
    public required double ElevationGain { get; init; }
    public required double LeadIn { get; init; }
    public required double LeadInElevationGain { get; init; }
    public required string? Restrictions { get; init; }
}
