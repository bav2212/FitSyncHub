using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftOffline;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Zwift.Services;
public class ZwiftAchievementsService
{
    private readonly ZwiftHttpClient _zwiftHttpClient;
    private readonly ZwiftOfflineHttpClient _zwiftOfflineHttpClient;
    private readonly ILogger<ZwiftAchievementsService> _logger;

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

    public ZwiftAchievementsService(
        ZwiftHttpClient zwiftHttpClient,
        ZwiftOfflineHttpClient zwiftOfflineHttpClient,
        ILogger<ZwiftAchievementsService> logger)
    {
        _zwiftHttpClient = zwiftHttpClient;
        _zwiftOfflineHttpClient = zwiftOfflineHttpClient;
        _logger = logger;
    }

    public async Task<List<ZwiftDataGameInfoAchievement>> GetMissingCyclingRouteAchievements(CancellationToken cancellationToken)
    {
        var gameInfo = await _zwiftOfflineHttpClient.GetGameInfo(cancellationToken);
        var routeAchievements = gameInfo.Achievements
            .Where(x => x.ImageUrl.EndsWith("RouteComplete.png"))
            .ToList();

        var mappedRouteAchievementsToRoutes = MapRouteAchievementsToRoutes(
            gameInfo.Maps, routeAchievements);
        var cyclingRouteAchievements = mappedRouteAchievementsToRoutes
            .Where(x => x.Value.Sports.Contains(ZwiftDataGameInfoSport.Cycling))
            .Select(x => x.Key)
            .ToList();

        var userAchievements = (await _zwiftHttpClient.GetAchievements(cancellationToken)).ToHashSet();

        return [.. cyclingRouteAchievements.Where(x => !userAchievements.Contains(x.Id))];
    }

    private Dictionary<ZwiftDataGameInfoAchievement, ZwiftDataGameInfoRoute> MapRouteAchievementsToRoutes(
        List<ZwiftDataGameInfoMap> maps,
        List<ZwiftDataGameInfoAchievement> routeAchievements)
    {
        var result = new Dictionary<ZwiftDataGameInfoAchievement, ZwiftDataGameInfoRoute>();

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
