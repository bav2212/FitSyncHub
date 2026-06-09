using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using FitSyncHub.Zwift.Models;
using FitSyncHub.Zwift.Providers.Abstractions;

namespace FitSyncHub.Zwift.Providers;

public sealed class ZwiftRoutesFromGameInfoProvider : IZwiftRoutesProvider
{
    private readonly Dictionary<string, string> _worldFriendlyNameMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        {"", Constants.ZwiftClimbPortalWorldName},
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

    private readonly ZwiftHttpClient _zwiftHttpClient;

    public ZwiftRoutesFromGameInfoProvider(ZwiftHttpClient zwiftHttpClient)
    {
        _zwiftHttpClient = zwiftHttpClient;
    }

    public async Task<List<ZwiftRouteModel>> GetRoutesInfo(CancellationToken cancellationToken)
    {
        var gameInfo = await _zwiftHttpClient.GetGameInfo(cancellationToken);

        return [.. ConvertToZwiftRouteModel(gameInfo)];
    }

    private IEnumerable<ZwiftRouteModel> ConvertToZwiftRouteModel(ZwiftGameInfoResponse gameInfo)
    {
        foreach (var map in gameInfo.Maps)
        {
            var worldName = _worldFriendlyNameMapping[map.Name];
            var isClimbPortal = worldName == Constants.ZwiftClimbPortalWorldName;

            foreach (var route in map.Routes)
            {
                var blockedForMeetups = route.BlockedForMeetups != 0;
                var distanceInMeters = isClimbPortal ? route.DistanceInMeters / 100 : route.DistanceInMeters;
                var ascentInMeters = isClimbPortal ? route.AscentInMeters / 100 : route.AscentInMeters;

                yield return new ZwiftRouteModel
                {
                    WorldName = worldName,
                    Name = route.Name,
                    Id = route.Id,
                    DistanceInMeters = distanceInMeters,
                    AscentInMeters = ascentInMeters,
                    LocKey = route.LocKey,
                    LevelLocked = route.LevelLocked,
                    PublicEventsOnly = route.PublicEventsOnly,
                    SupportedLaps = route.SupportedLaps,
                    LeadinAscentInMeters = route.LeadinAscentInMeters,
                    LeadinDistanceInMeters = route.LeadinDistanceInMeters,
                    BlockedForMeetups = blockedForMeetups,
                    Xp = route.Xp,
                    Duration = route.Duration,
                    Difficulty = route.Difficulty,
                    Sports = route.Sports,
                    PublishedOn = null,
                };
            }
        }
    }
}
