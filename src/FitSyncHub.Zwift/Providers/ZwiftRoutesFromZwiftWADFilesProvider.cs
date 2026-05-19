using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using FitSyncHub.Zwift.Models;
using FitSyncHub.Zwift.Providers.Abstractions;
using FitSyncHub.Zwift.Xml;
using FitSyncHub.Zwift.Xml.Models;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Zwift.Providers;

public sealed class ZwiftRoutesFromZwiftWADFilesProvider : IZwiftRoutesProvider
{
    // to align with gameInfo endpoint
    private static readonly Dictionary<string, string> s_nameMapping = new()
    {
        {"Classique", "London Classique"},
        {"Classique Reverse", "London Classique Reverse"},
        {"The PRL Half", "London PRL Half"},
        {"The PRL Full", "London PRL FULL"},
        {"2015 Worlds Course", "Richmond UCI Worlds"},
        {"Hilly Route", "Watopia Hilly Route"},
        {"Flat Route", "Watopia Flat Route"},
        {"Figure 8 Reverse", "Watopia Figure 8 Reverse"},
        {"Figure 8", "Watopia Figure 8"},
        {"Mountain Route", "Watopia Mountain Route"},
        {"Mountain 8", "Watopia Mountain 8"},
        {"The Pretzel", "Watopia Pretzel"},
    };

    private readonly ZwiftWorldsXmlFilesProvider _zwiftWorldsXmlFilesProvider;
    private readonly ILogger<ZwiftRoutesFromZwiftWADFilesProvider> _logger;

    public ZwiftRoutesFromZwiftWADFilesProvider(
        ZwiftWorldsXmlFilesProvider zwiftWorldsXmlFilesProvider,
        ILogger<ZwiftRoutesFromZwiftWADFilesProvider> logger)
    {
        _zwiftWorldsXmlFilesProvider = zwiftWorldsXmlFilesProvider;
        _logger = logger;
    }

    public async Task<List<ZwiftDataWorldRoutePair>> GetRoutesInfo(CancellationToken cancellationToken)
    {
        var worldRouteFilePaths = await _zwiftWorldsXmlFilesProvider.GetWorlsXmlFilesPaths(cancellationToken);

        return [
            .. ReadClimbPortaldRouteFilesAndParse(worldRouteFilePaths.ClimbPortalRoads),
            .. ReadRouteFilesAndParse(worldRouteFilePaths.RegularRoutes)
            ];
    }

    private static IEnumerable<ZwiftDataWorldRoutePair> ReadRouteFilesAndParse(
        List<ZwiftXmlFilesModelRegularRoutes> regularRoutes)
    {
        using var rootParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectRouteRoot>();

        foreach (var regularRoute in regularRoutes)
        {
            var zwiftInGameRoot = rootParser.Parse(regularRoute.FilePath);

            var route = zwiftInGameRoot.Route;
            var homedata = zwiftInGameRoot.Homedata;

            var routeName = s_nameMapping.TryGetValue(route.Name, out var mappedName)
                ? mappedName
                : route.Name;

            var publishedOn = !string.IsNullOrWhiteSpace(homedata?.PublishedOn)
                ? DateOnly.ParseExact(homedata.PublishedOn, "yyyy-MM-dd")
                : default(DateOnly?);

            yield return new ZwiftDataWorldRoutePair
            {
                WorldName = regularRoute.WorldName,
                Route = new ZwiftRouteModel
                {
                    Name = routeName,
                    Id = route.NameHash,
                    DistanceInMeters = route.DistanceInMeters,
                    AscentInMeters = route.AscentInMeters,
                    LocKey = route.LocKey,
                    LevelLocked = route.LevelLocked,
                    PublicEventsOnly = route.EventOnly || route.ZwiftEventOnly,
                    SupportedLaps = route.SupportedLaps,
                    LeadinAscentInMeters = route.LeadInAscentInMeters,
                    LeadinDistanceInMeters = route.LeadInDistanceInMeters,
                    BlockedForMeetups = route.BlockedForMeetups,
                    Xp = homedata?.Xp ?? 0,
                    Duration = homedata?.Duration ?? 0,
                    Difficulty = homedata?.Difficulty ?? 0,
                    Sports = route.SportType switch
                    {
                        -1 or 0 => [ZwiftGameInfoSport.Cycling, ZwiftGameInfoSport.Running, ZwiftGameInfoSport.Rowing],
                        1 => [ZwiftGameInfoSport.Cycling],
                        2 => [ZwiftGameInfoSport.Running],
                        3 => [ZwiftGameInfoSport.Cycling, ZwiftGameInfoSport.Running],
                        _ => throw new ArgumentException("Unknown sport type")
                    },
                    PublishedOn = publishedOn,
                }
            };
        }
    }

    private static IEnumerable<ZwiftDataWorldRoutePair> ReadClimbPortaldRouteFilesAndParse(
        List<ZwiftXmlFilesModelClimbPortalRoads> climbPortalRoads)
    {
        using var rootParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectClimbPortalRoadRoot>();

        foreach (var climbPortalRoad in climbPortalRoads)
        {
            var climbPortalRoadRoot = rootParser.Parse(climbPortalRoad.FilePath);

            var road = climbPortalRoadRoot.World.Roads.Single();
            var roadMetadata = road.Metadata;

            yield return new ZwiftDataWorldRoutePair
            {
                WorldName = "Climb Portal",
                Route = new ZwiftRouteModel
                {
                    Name = roadMetadata.UserFacingName,
                    Id = roadMetadata.Hash,
                    DistanceInMeters = roadMetadata.CourseLength / 100.0,
                    AscentInMeters = roadMetadata.CourseAscentF / 100.0,
                    LocKey = roadMetadata.PinIconPath,
                    LevelLocked = 0, // not present in this XML file
                    PublicEventsOnly = false, // not present in this XML file
                    SupportedLaps = false, // not present in this XML file
                    LeadinAscentInMeters = 0, // not present in this XML file
                    LeadinDistanceInMeters = 0, // not present in this XML file
                    BlockedForMeetups = false, // not present in this XML file
                    Xp = 0, // not present in this XML file
                    Duration = 0, // not present in this XML file
                    Difficulty = 0, // not present in this XML file
                    Sports = road.AllowedSport switch
                    {
                        -1 or 0 => [ZwiftGameInfoSport.Cycling, ZwiftGameInfoSport.Running, ZwiftGameInfoSport.Rowing],
                        1 => [ZwiftGameInfoSport.Cycling],
                        2 => [ZwiftGameInfoSport.Running],
                        3 => [ZwiftGameInfoSport.Cycling, ZwiftGameInfoSport.Running],
                        _ => throw new ArgumentException("Unknown sport type")
                    },
                    PublishedOn = null, // not present in this XML file
                }
            };
        }
    }
}
