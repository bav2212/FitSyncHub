using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Models.Requests.Events;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Events;
using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using FitSyncHub.Zwift.Models;
using FitSyncHub.Zwift.Providers.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Zwift.Services;

public sealed class ZwiftGameInfoService
{
    private readonly ZwiftHttpClient _zwiftHttpClient;
    private readonly IZwiftRoutesProvider _zwiftRoutesProvider;
    private readonly ILogger<ZwiftGameInfoService> _logger;
    private readonly bool _isDevelopment;
    private readonly HashSet<long> _knownCyclingRouteIdsWithoutAchievements = [
        2843604888,  //  Bologna-Bologna Time Trial
        2875658892,  //  Crit City-The Bell Lap
        2136907048,  //  France-France Classic Fondo
        4149936043,  //  France-Climb Portal-Mont Saint-Michel
        3101693166,  //  Innsbruck-InnsbruckConti
        474781994,   //  London-Greater London Loop Reverse
        1230300449,  //  London-Leith Hill After Party
        1788889233,  //  London-London Loop Reverse
        2165880404,  //  London-London 8 Reverse
        3569674525,  //  London-Keith Hill After Party
        3976402826,  //  London-Greatest London Loop Reverse
        352245150,   //  New York-Gotham Grind Reverse
        1763213625,  //  New York-The Highline Reverse
        2436095601,  //  New York-2022 Cycling Esports World Championships Route
        3687251774,  //  New York-Grand Central Circuit Reverse
        3774003351,  //  New York-LaGuardia Loop Reverse
        4194352271,  //  Richmond-Cobbled Climbs Reverse
        242381847,   //  Watopia-Zwift Gran Fondo
        387309391,   //  Watopia-Volcano Climb After Party
        1327147942,  //  Watopia-Zwift Gran Fondo 2022
        1397026382,  //  Watopia-Volcano Flat Reverse
        2218409282,  //  Watopia-WBR Climbing Series
        2681889340,  //  Watopia-Climb Portal -Volcano
        2900074211,  //  Watopia-Zwift Medio Fondo 2022
        2967612381,  //  Watopia-Road To Ruins Reverse
        3030004732,  //  Watopia-Power to the Portal
        3368626651,  //  Watopia-Zwift Bambino Fondo 2022
        3621162212,  //  Watopia-Zwift Bambino Fondo
        3748780161,  //  Watopia-Zwift Medio Fondo
        3811569265,  //  Watopia-Flat Route Reverse
        4107844490,  //  Watopia-Big Loop Reverse
        4263172118,  //  Watopia-Three Sisters Reverse
        620436060,   //  Yorkshire-Harrogate Circuit Reverse
    ];

    public ZwiftGameInfoService(
        ZwiftHttpClient zwiftHttpClient,
        IZwiftRoutesProvider zwiftRoutesProvider,
        ILogger<ZwiftGameInfoService> logger,
        IHostEnvironment hostingEnvironment)
    {
        _zwiftHttpClient = zwiftHttpClient;
        _zwiftRoutesProvider = zwiftRoutesProvider;
        _logger = logger;
        _isDevelopment = hostingEnvironment.IsDevelopment();
    }

    public async Task<Dictionary<ZwiftRouteModel, List<ZwiftEventResponse>>> GetUncompletedRouteToEventsMappingAchievements(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var gameInfo = await _zwiftHttpClient.GetGameInfo(cancellationToken);

        var zwiftAchievementsResolver = new ZwiftRouteAchievementResolver(gameInfo.Achievements, _isDevelopment);

        var routes = await _zwiftRoutesProvider.GetRoutesInfo(cancellationToken);
        var userAchievements = (await _zwiftHttpClient.GetPlayerAchievements(cancellationToken)).ToHashSet();

        var mappedCyclingRoutesToRouteAchievements = zwiftAchievementsResolver
            .MapRoutesToRouteAchievements(routes)
            // filter only cycling routes
            .Where(x => x.Key.Sports.Contains(ZwiftGameInfoSport.Cycling))
            .ToList();

        var routesWithUncompletedAchievementsDictionary = mappedCyclingRoutesToRouteAchievements
            .Where(x =>
            {
                var isNewRouteCouldPotentialyHaveArchievement = x.Value is null
                    && !_knownCyclingRouteIdsWithoutAchievements.Contains(x.Key.Id);

                var routeWithUncompletedAchievements = x.Value is not null
                    && !userAchievements.Contains(x.Value.Id);

                return isNewRouteCouldPotentialyHaveArchievement || routeWithUncompletedAchievements;
            })
            .Select(x => x.Key)
            .ToDictionary(x => x.Id, x => x);

        var events = await _zwiftHttpClient.GetEventFeedFullRangeBuggy(new ZwiftEventFeedRequest
        {
            From = from,
            To = to,
        }, cancellationToken);

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

    public async Task<List<ZwiftEventWithPreselectedBikeTuple>> GetZwiftEventsWithPreselectedBike(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var gameInfo = await _zwiftHttpClient.GetGameInfo(cancellationToken);
        var routes = await _zwiftRoutesProvider.GetRoutesInfo(cancellationToken);

        var bikeFramesDictionary = gameInfo.BikeFrames.ToDictionary(x => x.Id, x => x);
        var routesDictionary = routes.ToDictionary(x => x.Id, x => x);

        var events = await _zwiftHttpClient.GetEventFeedFullRangeBuggy(new ZwiftEventFeedRequest
        {
            From = from,
            To = to,
        }, cancellationToken);

        List<ZwiftEventWithPreselectedBikeTuple> result = [];
        foreach (var @event in events)
        {
            var bikeFrame = @event.BikeHash is not null && bikeFramesDictionary.TryGetValue(@event.BikeHash.Value, out var value1)
                ? value1
                : null;

            var routeInfo = routesDictionary[@event.RouteId];

            result.Add(new()
            {
                Event = @event,
                BikeFrame = bikeFrame,
                Route = routeInfo,
            });
        }

        return result;
    }

    public async Task<MappedUncompletedAchievementsModel> GetAchievementsState(
        CancellationToken cancellationToken)
    {
        var profileMe = await _zwiftHttpClient.GetProfileMe(cancellationToken);
        var gameInfo = await _zwiftHttpClient.GetGameInfo(cancellationToken);

        var zwiftAchievementsResolver = new ZwiftRouteAchievementResolver(gameInfo.Achievements, _isDevelopment);

        var routes = await _zwiftRoutesProvider.GetRoutesInfo(cancellationToken);
        var userAchievements = (await _zwiftHttpClient.GetPlayerAchievements(cancellationToken)).ToHashSet();

        var mappedRouteAchievementsToRoutes = zwiftAchievementsResolver.MapRouteAchievementsToRoutes(routes);

        return new MappedUncompletedAchievementsModel
        {
            AchievementLevel = profileMe.AchievementLevel / 100.0,
            CyclingRouteAchievementsToRouteMapping = GetRouteAchievementsToRouteMappingForSport(ZwiftGameInfoSport.Cycling),
            RunningRouteAchievementsToRouteMapping = GetRouteAchievementsToRouteMappingForSport(ZwiftGameInfoSport.Running),
            GeneralAchievements = [.. zwiftAchievementsResolver.GeneralAchievements.Select(ConvertToZwiftGameInfoAchievementState)]
        };

        Dictionary<ZwiftGameInfoAchievementState, ZwiftRouteModel> GetRouteAchievementsToRouteMappingForSport(ZwiftGameInfoSport sport)
        {
            return mappedRouteAchievementsToRoutes
                .Where(x => x.Value.Sports.Contains(sport))
                .ToDictionary(
                    x => ConvertToZwiftGameInfoAchievementState(x.Key),
                    x => x.Value
                );
        }

        ZwiftGameInfoAchievementState ConvertToZwiftGameInfoAchievementState(ZwiftGameInfoAchievement achievement)
        {
            return new ZwiftGameInfoAchievementState
            {
                Achievement = achievement,
                IsAchieved = userAchievements.Contains(achievement.Id)
            };
        }
    }
}

public sealed record ZwiftGameInfoAchievementState
{
    public required ZwiftGameInfoAchievement Achievement { get; init; }
    public required bool IsAchieved { get; init; }
}

public sealed record MappedUncompletedAchievementsModel
{
    public required Dictionary<ZwiftGameInfoAchievementState, ZwiftRouteModel> CyclingRouteAchievementsToRouteMapping { get; init; }
    public required Dictionary<ZwiftGameInfoAchievementState, ZwiftRouteModel> RunningRouteAchievementsToRouteMapping { get; init; }
    public required List<ZwiftGameInfoAchievementState> GeneralAchievements { get; init; }
    public required double AchievementLevel { get; init; }
}

public sealed record ZwiftEventWithPreselectedBikeTuple
{
    public required ZwiftEventResponse Event { get; init; }
    public required ZwiftGameInfoBikeFrame? BikeFrame { get; init; }
    public required ZwiftRouteModel Route { get; init; }
}
