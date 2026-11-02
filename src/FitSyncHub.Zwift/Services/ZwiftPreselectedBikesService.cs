using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Models.Requests.Events;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Events;
using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using FitSyncHub.Zwift.Models;
using FitSyncHub.Zwift.Providers.Abstractions;

namespace FitSyncHub.Zwift.Services;

public sealed class ZwiftPreselectedBikesService
{
    private readonly ZwiftHttpClient _zwiftHttpClient;
    private readonly IZwiftRoutesProvider _zwiftRoutesProvider;

    public ZwiftPreselectedBikesService(
      ZwiftHttpClient zwiftHttpClient,
      IZwiftRoutesProvider zwiftRoutesProvider)
    {
        _zwiftHttpClient = zwiftHttpClient;
        _zwiftRoutesProvider = zwiftRoutesProvider;
    }

    public async Task<List<ZwiftEventWithPreselectedBikeResponse>> GetEvents(
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

        List<ZwiftEventWithPreselectedBikeResponse> result = [];
        foreach (var @event in events)
        {
            foreach (var eventSubgroup in @event.EventSubgroups)
            {
                var bikeFrame = eventSubgroup.BikeHash is not null && bikeFramesDictionary.TryGetValue(eventSubgroup.BikeHash.Value, out var value1)
                    ? value1
                    : null;

                var routeInfo = routesDictionary[eventSubgroup.RouteId];

                result.Add(new()
                {
                    EventId = @event.Id,
                    EventSubgroup = eventSubgroup,
                    BikeFrame = bikeFrame,
                    Route = routeInfo,
                });
            }
        }

        return result;
    }

    public sealed record ZwiftEventWithPreselectedBikeResponse
    {
        public required long EventId { get; init; }
        public required ZwiftEventSubgroupResponse EventSubgroup { get; init; }
        public required ZwiftGameInfoBikeFrame? BikeFrame { get; init; }
        public required ZwiftRouteModel Route { get; init; }
    }
}
