using System.Text.Json;
using System.Text.Json.Serialization;
using FitSyncHub.Zwift.Providers;
using FitSyncHub.Zwift.Xml;
using FitSyncHub.Zwift.Xml.Models;

namespace FitSyncHub.Zwift.Sauce;

public sealed class ZwiftSauceService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly ZwiftWorldsXmlFilesProvider _zwiftWorldsXmlFilesProvider;
    private readonly string _sauceDataPath;

    public ZwiftSauceService(ZwiftWorldsXmlFilesProvider zwiftWorldsXmlFilesProvider)
    {
        _zwiftWorldsXmlFilesProvider = zwiftWorldsXmlFilesProvider;
        _sauceDataPath = Environment.GetEnvironmentVariable("SAUCE_DATA_PATH")
            ?? throw new InvalidOperationException("SAUCE_DATA_PATH environment variable is not set.");

    }

    public async Task InitializeSauceData(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_sauceDataPath))
        {
            throw new InvalidOperationException($"SAUCE_DATA_PATH '{_sauceDataPath}' does not exist.");
        }

        var worldRouteFilePaths = await _zwiftWorldsXmlFilesProvider.GetWorlsXmlFilesPaths(cancellationToken);

        using var roadParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectRoadRoot>();
        using var entitiesParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectEntitiesRoot>();
        using var roadStyleParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectRoadStyleRoot>();

        foreach (var world in worldRouteFilePaths.Worlds)
        {
            var zwiftParsedRoad = roadParser.Parse(world.Road.FilePath);
            var zwiftParsedRoadStyle = roadStyleParser.Parse(world.RoadStyle.FilePath);

            var roads = ZwiftSauceRoadConverter.Convert(
                world.WorldId,
                zwiftParsedRoad,
                zwiftParsedRoadStyle);

            var json = JsonSerializer.Serialize(roads, _jsonOptions);
            var path = @$"{_sauceDataPath}\worlds\{world.WorldId}\roads.json";
            await File.WriteAllTextAsync(path, json, cancellationToken);
        }

        foreach (var world in worldRouteFilePaths.Worlds)
        {
            var zwiftParsedEntities = entitiesParser.Parse(world.Entities.FilePath);

            var segments = ZwiftSauceSegmentsConverter.Convert(
                world.WorldId,
                zwiftParsedEntities);

            var json = JsonSerializer.Serialize(segments, _jsonOptions);
            var path = @$"{_sauceDataPath}\worlds\{world.WorldId}\segments.json";
            await File.WriteAllTextAsync(path, json, cancellationToken);
        }

        using var routeParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectRouteRoot>();
        List<ZwiftSauceRoute> result = [];

        foreach (var world in worldRouteFilePaths.Worlds)
        {
            foreach (var regularRoute in world.RegularRoutes)
            {
                var zwiftParsedRoute = routeParser.Parse(regularRoute.FilePath);
                result.Add(ZwiftSauceRouteConverter.Convert(zwiftParsedRoute, world.WorldId));
            }
        }

        var routesJson = JsonSerializer.Serialize(result, _jsonOptions);
        var routesPath = @$"{_sauceDataPath}\routes.json";
        await File.WriteAllTextAsync(routesPath, routesJson, cancellationToken);
    }
}
