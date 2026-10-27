using System.Globalization;
using System.Text.Json;
using System.Xml;

namespace FitSyncHub.Zwift.UnitTests;

public static class ZwiftSauceConverterTestData
{
    private const string SauceWorldsPath = @"Data\Sauce\worlds";
    private const string ZwiftWorldsPath = @"Data\Zwift\Worlds";

    public static TheoryData<uint, string, string, string> Roads { get; } = CreateRoadData();
    public static TheoryData<uint, string, string> Segments { get; } = CreateSegmentData();
    public static TheoryData<uint, string, string> Routes { get; } = CreateRouteData();

    private static TheoryData<uint, string, string, string> CreateRoadData()
    {
        TheoryData<uint, string, string, string> data = [];

        foreach (var (worldId, saucePath, zwiftPath) in GetWorldFilePairs("roads.json", "road.xml"))
        {
            var roadStylePath = Path.Combine(Path.GetDirectoryName(zwiftPath)!, "roadstyle.xml");
            data.Add(worldId, zwiftPath, roadStylePath, saucePath);
        }

        return data;
    }

    private static TheoryData<uint, string, string> CreateSegmentData()
    {
        TheoryData<uint, string, string> data = [];

        foreach (var (worldId, saucePath, zwiftPath) in GetWorldFilePairs("segments.json", "entities.xml"))
        {
            data.Add(worldId, zwiftPath, saucePath);
        }

        return data;
    }

    private static IEnumerable<(uint WorldId, string SaucePath, string ZwiftPath)> GetWorldFilePairs(
        string sauceFileName,
        string zwiftFileName)
    {
        var sauceFilesByWorld = Directory
            .EnumerateFiles(SauceWorldsPath, sauceFileName, new EnumerationOptions { RecurseSubdirectories = true })
            .ToDictionary(GetSauceWorldId);
        var zwiftFilesByWorld = Directory
            .EnumerateFiles(ZwiftWorldsPath, zwiftFileName, new EnumerationOptions { RecurseSubdirectories = true })
            .ToDictionary(GetZwiftWorldId);

        foreach (var (worldId, saucePath) in sauceFilesByWorld.OrderBy(pair => pair.Key))
        {
            if (!zwiftFilesByWorld.TryGetValue(worldId, out var zwiftPath))
            {
                throw new InvalidOperationException(
                    $"No Zwift '{zwiftFileName}' fixture was found for world {worldId}.");
            }

            yield return (worldId, saucePath, zwiftPath);
        }
    }

    private static TheoryData<uint, string, string> CreateRouteData()
    {
        var routeManifests = new Dictionary<long, string>();
        using (var routesJson = JsonDocument.Parse(File.ReadAllText(@"Data\Sauce\routes.json")))
        {
            foreach (var route in routesJson.RootElement.EnumerateArray())
            {
                var routeId = route.GetProperty("id").GetInt64();
                routeManifests.Add(routeId, route.GetProperty("manifest").GetRawText());
            }
        }

        var zwiftRoutePaths = Directory
            .EnumerateFiles(ZwiftWorldsPath, "routes*.xml", new EnumerationOptions { RecurseSubdirectories = true })
            .ToArray();

        if (routeManifests.Count != zwiftRoutePaths.Length)
        {
            throw new InvalidOperationException(
                $"The number of route manifests ({routeManifests.Count}) does not match the number of route XML files ({zwiftRoutePaths.Length}).");
        }

        TheoryData<uint, string, string> data = [];
        foreach (var routePath in zwiftRoutePaths)
        {
            var worldId = GetZwiftWorldId(routePath);
            var routeId = GetRouteId(routePath);

            if (!routeManifests.TryGetValue(routeId, out var manifestJson))
            {
                throw new InvalidOperationException(
                    $"No Sauce route manifest was found for route {routeId} in '{routePath}'.");
            }

            data.Add(worldId, routePath, manifestJson);
        }

        return data;
    }

    private static long GetRouteId(string routePath)
    {
        using var reader = XmlReader.Create(routePath, new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            ConformanceLevel = ConformanceLevel.Fragment
        });

        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != "route")
            {
                continue;
            }

            var nameHash = reader.GetAttribute("nameHash")
                ?? throw new InvalidDataException($"Route in '{routePath}' is missing its nameHash attribute.");
            return long.Parse(nameHash, CultureInfo.InvariantCulture);
        }

        throw new InvalidDataException($"No route element was found in '{routePath}'.");
    }

    private static uint GetSauceWorldId(string path)
    {
        var directoryName = Path.GetFileName(Path.GetDirectoryName(path))
            ?? throw new InvalidDataException($"Could not determine the Sauce world directory for '{path}'.");
        return uint.Parse(directoryName, CultureInfo.InvariantCulture);
    }

    private static uint GetZwiftWorldId(string path)
    {
        var worldDirectory = Path.GetFileName(Path.GetDirectoryName(path))
            ?? throw new InvalidDataException($"Could not determine the Zwift world directory for '{path}'.");
        if (worldDirectory == "routes")
        {
            worldDirectory = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path)))
                ?? throw new InvalidDataException($"Could not determine the Zwift world directory for '{path}'.");
        }

        const string Prefix = "world";
        if (!worldDirectory.StartsWith(Prefix, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Expected a Zwift world directory but found '{worldDirectory}'.");
        }

        return uint.Parse(worldDirectory[Prefix.Length..], CultureInfo.InvariantCulture);
    }
}
