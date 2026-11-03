using System.Security.Cryptography;
using System.Text.Json;
using FitSyncHub.Zwift.HttpClients;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Zwift.Providers;

public class ZwiftWorldsXmlFilesProvider
{
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
        {"10", "France"},
        {"11", "Paris"},
        {"12", "Gravel Mountain"},
        {"13", "Scotland"},
    };

    private readonly string _zwiftInstallationPath = @"C:\Program Files (x86)\Zwift";
    private readonly string _zwiftWorldsPath;

    private readonly ILogger<ZwiftWorldsXmlFilesProvider> _logger;

    private readonly ZwiftWadDecoder _zwiftWadDecoder;
    private readonly ZwiftMochiHttpClient _zwiftMochiHttpClient;
    private readonly string _unpackedWADFilesDirectory;
    private readonly string _unpackedWADFilesStateFilePath;
    private readonly string _mochiFilesDirectory;
    private readonly string _mochiStateFilePath;

    public ZwiftWorldsXmlFilesProvider(
        ZwiftWadDecoder zwiftWadDecoder,
        ZwiftMochiHttpClient zwiftMochiHttpClient,
        ILogger<ZwiftWorldsXmlFilesProvider> logger)
    {
        _zwiftWadDecoder = zwiftWadDecoder;
        _zwiftMochiHttpClient = zwiftMochiHttpClient;
        _logger = logger;

        _zwiftWorldsPath = Path.Combine(_zwiftInstallationPath, "assets", "Worlds");

        _unpackedWADFilesDirectory = Path.Combine(Path.GetTempPath(), "ZwiftWADFilesUnpacked");
        _unpackedWADFilesStateFilePath = Path.Combine(_unpackedWADFilesDirectory, "state.json");
        _mochiFilesDirectory = Path.Combine(_unpackedWADFilesDirectory, "Mochi");
        _mochiStateFilePath = Path.Combine(_unpackedWADFilesDirectory, "mochi_state.json");
    }

    public async Task<ZwiftXmlFilesModel> GetWorlsXmlFilesPaths(CancellationToken cancellationToken)
    {
        await UnpackWADFiles(cancellationToken);
        await LoadMochiFiles(cancellationToken);

        var worldsDictionary = new Dictionary<long, ZwiftXmlFilesWorldData>();
        var climbPortalRoads = new List<ZwiftXmlFilesRoadItem>();

        var filePaths = Directory.EnumerateFiles(Path.Combine(_unpackedWADFilesDirectory, "Worlds"),
                "*.xml", new EnumerationOptions { RecurseSubdirectories = true });

        foreach (var filePath in filePaths)
        {
            var pathParts = GetRelativePathParts(filePath);
            var isWorldRelatedFilePath = pathParts.Length > 0
                && pathParts[0].StartsWith("world", StringComparison.OrdinalIgnoreCase);

            var isWorldRoadFilePath = isWorldRelatedFilePath
                && pathParts[1] == "road.xml";

            var isWorldEntitiesFilePath = isWorldRelatedFilePath
                && pathParts[1] == "entities.xml";

            var isWorldRoadStyleFilePath = isWorldRelatedFilePath
                && pathParts[1] == "roadstyle.xml";

            var isWorldRouteFilePath = isWorldRelatedFilePath
                && pathParts.Length > 2
                && pathParts[1] == "routes";

            if (isWorldRelatedFilePath)
            {
                var (worldId, worldName) = GetWorldInfoFromFilePath(pathParts);

                if (!worldsDictionary.TryGetValue(worldId, out var worldData))
                {
                    worldData = new ZwiftXmlFilesWorldData
                    {
                        WorldId = worldId,
                        WorldName = worldName,
                        RegularRoutes = []
                    };
                    worldsDictionary[worldId] = worldData;
                }

                if (isWorldRoadFilePath)
                {
                    worldData.Road = new ZwiftXmlFilesRoadItem
                    {
                        FilePath = filePath
                    };
                    continue;
                }

                if (isWorldEntitiesFilePath)
                {
                    worldData.Entities = new ZwiftXmlFilesRoadItem
                    {
                        FilePath = filePath
                    };
                    continue;
                }

                if (isWorldRoadStyleFilePath)
                {
                    worldData.RoadStyle = new ZwiftXmlFilesRoadItem
                    {
                        FilePath = filePath
                    };
                    continue;
                }

                if (isWorldRouteFilePath)
                {
                    worldData.RegularRoutes.Add(new ZwiftXmlFilesRoadItem
                    {
                        FilePath = filePath
                    });

                    continue;
                }
            }

            var isClimbPortalFilePath = pathParts[0] == "portal"
                && pathParts.Length > 1
                && pathParts[1].StartsWith("road_", StringComparison.OrdinalIgnoreCase);
            if (isClimbPortalFilePath)
            {
                climbPortalRoads.Add(new ZwiftXmlFilesRoadItem
                {
                    FilePath = filePath
                });
                continue;
            }
        }

        var mochiFilePaths = Directory
            .EnumerateFiles(_mochiFilesDirectory, "*.xml", new EnumerationOptions { RecurseSubdirectories = true })
            .Select(x => new ZwiftXmlFilesRoadItem { FilePath = x })
            .ToList();

        return new ZwiftXmlFilesModel
        {
            Worlds = [.. worldsDictionary.Values],
            ClimbPortalRoads = climbPortalRoads,
            MochiRoads = mochiFilePaths
        };
    }

    private async Task LoadMochiFiles(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_mochiFilesDirectory))
        {
            Directory.CreateDirectory(_mochiFilesDirectory);
        }

        Dictionary<string, MochiRoadStateItem> mochiRideNameToRoadSizeMapping = [];
        if (File.Exists(_mochiStateFilePath))
        {
            var mochiStateJson = await File.ReadAllTextAsync(_mochiStateFilePath, cancellationToken);
            var mochiStateList = JsonSerializer
                .Deserialize<List<MochiRoadStateItem>>(mochiStateJson)!;
            mochiRideNameToRoadSizeMapping = mochiStateList
                .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        }

        var mochiCatalogResponse = await _zwiftMochiHttpClient.GetMochiCatalogResponse(cancellationToken);
        foreach (var mochiCatalogCollections in mochiCatalogResponse.Collections)
        {
            foreach (var mochiCatalogRide in mochiCatalogCollections.Rides)
            {
                if (mochiCatalogRide is not { } || mochiCatalogRide.Route is not { } || mochiCatalogRide.Name is not { })
                {
                    continue;
                }

                var key = mochiCatalogRide.Name;
                if (mochiRideNameToRoadSizeMapping.TryGetValue(key, out var mochiRideRoadStateItem)
                    && mochiRideRoadStateItem.Crc == mochiCatalogRide.Route.Crc
                    && mochiRideRoadStateItem.Size == mochiCatalogRide.Route.Size)
                {
                    _logger.LogWarning("Do not need to load mochi file again: {MochiRideName}", key);
                    continue;
                }

                var destination = Path.Combine(_mochiFilesDirectory, $"route_{mochiCatalogRide.Name}.xml");
                await _zwiftMochiHttpClient.DownloadMochiItem(mochiCatalogRide.Route, destination, cancellationToken);
                await Task.Delay(100, cancellationToken); // Add a small delay to avoid overwhelming the server)

                mochiRideNameToRoadSizeMapping[mochiCatalogRide.Name] = new MochiRoadStateItem
                {
                    Name = mochiCatalogRide.Name,
                    Crc = mochiCatalogRide.Route.Crc,
                    Size = mochiCatalogRide.Route.Size
                };
            }
        }

        var updatedMochiStateList = JsonSerializer.Serialize(mochiRideNameToRoadSizeMapping.Values.ToList())!;
        await File.WriteAllTextAsync(_mochiStateFilePath, updatedMochiStateList, cancellationToken);
    }

    private static (uint worldId, string worldName) GetWorldInfoFromFilePath(string[] pathParts)
    {
        var worldId = pathParts[0][5..]; // trim "world"
        var worldName = s_worldIdToNameMapping[worldId];

        return (uint.Parse(worldId), worldName);
    }

    private async Task UnpackWADFiles(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_unpackedWADFilesDirectory))
        {
            Directory.CreateDirectory(_unpackedWADFilesDirectory);
        }

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
        foreach (var filePath in Directory.EnumerateFiles(_zwiftWorldsPath,
            "*.wad", new EnumerationOptions() { RecurseSubdirectories = true }))
        {
            var pathParts = GetRelativePathParts(filePath);
            var fileName = pathParts[^1];
            if (fileName != "data_1.wad" && fileName != "roads.wad")
            {
                continue;
            }

            var isWorldRelatedFilePath = pathParts.Length > 0
                && pathParts[^2].StartsWith("world", StringComparison.OrdinalIgnoreCase)
                && pathParts[^2] != "Worlds";
            if (isWorldRelatedFilePath)
            {
                var worldId = pathParts[^2][5..]; // trim "world"
                if (!s_worldIdToNameMapping.ContainsKey(worldId))
                {
                    continue;
                }
            }

            var hash = ComputeHash(filePath);
            if (unpackedWADFilesStateDictionary.TryGetValue(filePath, out var unpackedWADFilesStateItem)
                && unpackedWADFilesStateItem.Hash == hash)
            {
                continue;
            }

            _zwiftWadDecoder.Unpack(filePath, _unpackedWADFilesDirectory);
            unpackedWADFilesStateDictionary[filePath] = new UnpackedWADFilesStateItem { FilePath = filePath, Hash = hash };
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

        return Path.GetRelativePath(basePath, filePath).Split(Path.DirectorySeparatorChar);
    }

    private sealed record UnpackedWADFilesStateItem
    {
        public required string FilePath { get; init; }
        public required string Hash { get; init; }
    }

    private sealed record MochiRoadStateItem
    {
        public required string Name { get; init; }
        public required long Crc { get; init; }
        public required long Size { get; init; }
    }
}

public sealed record ZwiftXmlFilesModel
{
    public required List<ZwiftXmlFilesWorldData> Worlds { get; init; }
    public required List<ZwiftXmlFilesRoadItem> ClimbPortalRoads { get; init; }
    public required List<ZwiftXmlFilesRoadItem> MochiRoads { get; init; }
}

public sealed record ZwiftXmlFilesWorldData
{
    public required uint WorldId { get; set; }
    public required string WorldName { get; set; }
    public ZwiftXmlFilesRoadItem Road { get; set; } = null!;
    public ZwiftXmlFilesRoadItem Entities { get; set; } = null!;
    public ZwiftXmlFilesRoadItem RoadStyle { get; set; } = null!;
    public required List<ZwiftXmlFilesRoadItem> RegularRoutes { get; set; }
}

public sealed record ZwiftXmlFilesRoadItem
{
    public required string FilePath { get; init; }
}
