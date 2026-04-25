using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Models.Requests.Events;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Events;
using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using FitSyncHub.Zwift.Models;
using FitSyncHub.Zwift.Providers.Abstractions;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Zwift.Services;

public sealed class ZwiftGameInfoService
{
    private readonly ZwiftHttpClient _zwiftHttpClient;
    private readonly IZwiftRoutesProvider _zwiftRoutesProvider;
    private readonly ILogger<ZwiftGameInfoService> _logger;

    public ZwiftGameInfoService(
        ZwiftHttpClient zwiftHttpClient,
        IZwiftRoutesProvider zwiftRoutesProvider,
        ILogger<ZwiftGameInfoService> logger)
    {
        _zwiftHttpClient = zwiftHttpClient;
        _zwiftRoutesProvider = zwiftRoutesProvider;
        _logger = logger;
    }

    public async Task<Dictionary<ZwiftRouteModel, List<ZwiftEventResponse>>> GetUncompletedRouteToEventsMappingAchievements(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var gameInfo = await _zwiftHttpClient.GetGameInfo(cancellationToken);

        var zwiftAchievementsResolver = new ZwiftRouteAchievementResolver(gameInfo.Achievements);

        var routes = await _zwiftRoutesProvider.GetRoutesInfo(cancellationToken);
        var userAchievements = (await _zwiftHttpClient.GetPlayerAchievements(cancellationToken)).ToHashSet();

        var routesWithUncompletedAchievementsDictionary = zwiftAchievementsResolver
            .MapRoutesToRouteAchievements(routes)
            // filter only cycling routes
            .Where(x => x.Key.Sports.Contains(ZwiftGameInfoSport.Cycling)
                && x.Value is not null && !userAchievements.Contains(x.Value.Id))
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
        var routesDictionary = routes.ToDictionary(x => x.Route.Id, x => x);

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
                Route = routeInfo.Route,
            });
        }

        return result;
    }

    public async Task<MappedUncompletedAchievementsModel> GetAchievementsState(
        CancellationToken cancellationToken)
    {
        var profileMe = await _zwiftHttpClient.GetProfileMe(cancellationToken);
        var gameInfo = await _zwiftHttpClient.GetGameInfo(cancellationToken);

        var zwiftAchievementsResolver = new ZwiftRouteAchievementResolver(gameInfo.Achievements);

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
