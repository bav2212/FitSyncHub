using System.Security.Cryptography;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using FitSyncHub.Zwift.Models;
using FitSyncHub.Zwift.Providers.Abstractions;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Zwift.Providers;

public class ZwiftRoutesFromZwiftWADFilesProvider : IZwiftRoutesProvider
{
    private readonly string _zwiftWorldsPath = @"C:\Program Files (x86)\Zwift\assets\Worlds";

    private static readonly Dictionary<string, string> s_worldIdToNameMapping = new()
    {
        {"1",  "Watopia"},
        {"2",  "Richmond"},
        {"3",  "London"},
        {"4",  "New York"},
        {"5",  "Innsbruck"},
        {"6",  "Bologna"},
        {"7",  "Yorkshire"},
        {"8",  "Crit City"},
        {"9",  "Makuri Islands"},
        {"10",  "France"},
        {"11",  "Paris"},
        {"12",  "Gravel Mountain"},
        {"13",  "Scotland"},
    };

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

    private readonly ZwiftWadDecoder _zwiftWadDecoder;
    private readonly ILogger<ZwiftRoutesFromZwiftWADFilesProvider> _logger;
    private readonly string _unpackedWADFilesDirectory;
    private readonly string _unpackedWADFilesStateFilePath;

    public ZwiftRoutesFromZwiftWADFilesProvider(
        ZwiftWadDecoder zwiftWadDecoder,
        ILogger<ZwiftRoutesFromZwiftWADFilesProvider> logger)
    {
        _zwiftWadDecoder = zwiftWadDecoder;
        _logger = logger;

        _unpackedWADFilesDirectory = Path.Combine(Path.GetTempPath(), "ZwiftWADFilesUnpacked");
        _unpackedWADFilesStateFilePath = Path.Combine(_unpackedWADFilesDirectory, "state.json");
    }

    public async Task<List<ZwiftDataWorldRoutePair>> GetRoutesInfo(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_unpackedWADFilesDirectory))
        {
            Directory.CreateDirectory(_unpackedWADFilesDirectory);
        }

        await UnpackWADFiles(cancellationToken);

        var worldRouteFilePaths = Directory.EnumerateFiles(
            Path.Combine(_unpackedWADFilesDirectory, "Worlds"),
            "*.xml",
            new EnumerationOptions { RecurseSubdirectories = true })
            .Where(filePath =>
            {
                var pathParts = GetRelativePathParts(filePath);

                return pathParts[0].StartsWith("world", StringComparison.OrdinalIgnoreCase)
                    && pathParts[1] == "routes";
            });

        // TODO
        // If need climb portal, use reference https://github.com/zoffline/zwift-offline/blob/master/scripts/get_climbs.py as reference

        return [.. ReadRouteFilesAndParse(worldRouteFilePaths)];
    }

    private async Task UnpackWADFiles(CancellationToken cancellationToken)
    {
        Dictionary<string, UnpackedWADFilesStateItem> unpackedWADFilesStateDictionary = [];
        if (File.Exists(_unpackedWADFilesStateFilePath))
        {
            var unpackedWADFilesStateJson = await File.ReadAllTextAsync(_unpackedWADFilesStateFilePath, cancellationToken);
            var unpackedWADFilesStateList = JsonSerializer
                .Deserialize<List<UnpackedWADFilesStateItem>>(unpackedWADFilesStateJson)!;
            unpackedWADFilesStateDictionary = unpackedWADFilesStateList
                .ToDictionary(x => x.FilePath, StringComparer.OrdinalIgnoreCase);
        }

        //inspired by https://github.com/zoffline/zwift-offline/blob/master/scripts/get_start_lines.py#L29
        // do it inside 'if block' cause service is singleton and _unpackedWADFilesDirectory will be the same during application run
        foreach (var file in Directory.EnumerateFiles(_zwiftWorldsPath,
            "data_1.wad", new EnumerationOptions() { RecurseSubdirectories = true }))
        {
            var hash = ComputeHash(file);
            if (unpackedWADFilesStateDictionary.TryGetValue(file, out var unpackedWADFilesStateItem)
                && unpackedWADFilesStateItem.Hash == hash)
            {
                continue;
            }

            _zwiftWadDecoder.Unpack(file, _unpackedWADFilesDirectory);
            unpackedWADFilesStateDictionary[file] = new UnpackedWADFilesStateItem { FilePath = file, Hash = hash };
        }

        var updatedUnpackedWADFilesStateList = JsonSerializer.Serialize(unpackedWADFilesStateDictionary.Values.ToList())!;
        await File.WriteAllTextAsync(_unpackedWADFilesStateFilePath, updatedUnpackedWADFilesStateList, cancellationToken);
    }

    // move to common if need
    private static string ComputeHash(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var myHash = md5.ComputeHash(stream);

        return Convert.ToBase64String(myHash);
    }

    private string[] GetRelativePathParts(string filePath)
    {
        var basePath = Path.Combine(_unpackedWADFilesDirectory, "Worlds");

        return [.. Path.GetRelativePath(basePath, filePath).Split(Path.DirectorySeparatorChar)];
    }

    private IEnumerable<ZwiftDataWorldRoutePair> ReadRouteFilesAndParse(
        IEnumerable<string> filePaths)
    {
        var routeSerializer = new XmlSerializer(typeof(ZwiftInGameRouteXmlElementDTO));
        var homemadeSerializer = new XmlSerializer(typeof(ZwiftInGameHomedataXmlElementDTO));

        foreach (var filePath in filePaths)
        {
            var worldId = GetRelativePathParts(filePath)[0][5..]; // trim "world"
            var worldName = s_worldIdToNameMapping[worldId];

            using var reader = XmlReader.Create(filePath, new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
                // THIS is the important part: allow multiple top-level elements
                ConformanceLevel = ConformanceLevel.Fragment,
            });

            ZwiftInGameRouteXmlElementDTO? routeXmlDTO = default;
            ZwiftInGameHomedataXmlElementDTO? homedataXmlDTO = default;

            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                switch (reader.Name)
                {
                    case "route":
                        {
                            using var sub = reader.ReadSubtree();
                            sub.Read(); // move into element
                            routeXmlDTO = routeSerializer.Deserialize(sub) as ZwiftInGameRouteXmlElementDTO
                                ?? throw new InvalidDataException("Can't deserialize route");
                            break;
                        }
                    case "homedata":
                        {
                            using var sub = reader.ReadSubtree();
                            sub.Read(); // move into element
                            homedataXmlDTO = homemadeSerializer.Deserialize(sub) as ZwiftInGameHomedataXmlElementDTO
                                ?? throw new InvalidDataException("Can't deserialize homedata");
                            break;

                        }
                }
            }

            if (routeXmlDTO == null || homedataXmlDTO == null)
            {
                _logger.LogWarning("Skipping route file {FilePath} due to missing route or homedata, route: {Route}",
                    filePath,
                    routeXmlDTO?.Name);
                continue;
            }

            var zwiftInGameRoot = new ZwiftInGameRootXmlObject { Route = routeXmlDTO, Homedata = homedataXmlDTO };

            yield return new ZwiftDataWorldRoutePair()
            {
                WorldName = worldName,
                Route = MapZwiftInGameRootXmlDTOToRoute(zwiftInGameRoot)
            };

        }
    }

    private static ZwiftRouteModel MapZwiftInGameRootXmlDTOToRoute(ZwiftInGameRootXmlObject zwiftInGameRoot)
    {
        var route = zwiftInGameRoot.Route;
        var homedata = zwiftInGameRoot.Homedata;

        var routeName = s_nameMapping.TryGetValue(route.Name, out var mappedName)
            ? mappedName
            : route.Name;

        var publishedOn = !string.IsNullOrWhiteSpace(homedata.PublishedOn)
            ? DateOnly.ParseExact(homedata.PublishedOn, "yyyy-MM-dd")
            : default(DateOnly?);

        return new ZwiftRouteModel()
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
            Xp = homedata.Xp,
            Duration = homedata.Duration,
            Difficulty = homedata.Difficulty,
            Sports = route.SportType switch
            {
                -1 or 0 => [ZwiftGameInfoSport.Cycling, ZwiftGameInfoSport.Running, ZwiftGameInfoSport.Rowing],
                1 => [ZwiftGameInfoSport.Cycling],
                2 => [ZwiftGameInfoSport.Running],
                3 => [ZwiftGameInfoSport.Cycling, ZwiftGameInfoSport.Running],
                _ => throw new ArgumentException("Unknown sport type")
            },
            PublishedOn = publishedOn,
        };
    }

    private record UnpackedWADFilesStateItem
    {
        public required string FilePath { get; init; }
        public required string Hash { get; init; }
    }
}
