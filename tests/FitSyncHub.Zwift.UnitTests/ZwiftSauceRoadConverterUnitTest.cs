using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FitSyncHub.Zwift.Sauce;
using FitSyncHub.Zwift.Xml;
using FitSyncHub.Zwift.Xml.Models;

namespace FitSyncHub.Zwift.UnitTests;

public sealed class ZwiftSauceRoadConverterUnitTest
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
                {
                    static typeInfo =>
                    {
                        if (typeInfo.Kind != JsonTypeInfoKind.Object)
                        {
                            return;
                        }

                        foreach (var propertyInfo in typeInfo.Properties)
                        {
                            // Strip IsRequired constraint from every property.
                            propertyInfo.IsRequired = false;
                        }
                    }
                }
        }
    };


    [Theory]
    [ClassData(typeof(TestData))]
    public void WorkCorrectly(
        string zwiftWorldRoadPath, string zwiftWorldRoadStylePath, string expectedSauceRoadPath)
    {
        using var roadParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectRoadRoot>();
        var zwiftInGameRoot = roadParser.Parse(zwiftWorldRoadPath);

        using var roadStyleParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectRoadStyleRoot>();
        var zwiftRoadStyleRoot = roadStyleParser.Parse(zwiftWorldRoadStylePath);

        var actual = ZwiftSauceRoadConverter
            .Convert(zwiftInGameRoot, zwiftRoadStyleRoot);

        var sauceRoadJson = File.ReadAllText(expectedSauceRoadPath);
        var sauceRoads = JsonSerializer.Deserialize<ZwiftSauceRoad[]>(sauceRoadJson, _jsonSerializerOptions)!;
        var expected = sauceRoads
            .Select(road => road with { Segments = [], Styles = [] })
            .ToArray();

        Assert.Equal(expected.Length, actual.Length);
        for (var index = 0; index < expected.Length; index++)
        {
            Assert.Equal(expected[index].Id, actual[index].Id);
            Assert.Equal(expected[index].DefaultStyle, actual[index].DefaultStyle);
            Assert.Equal(expected[index].IsAvailable, actual[index].IsAvailable);
            Assert.Equal(expected[index].Looped, actual[index].Looped);
            Assert.Equal(expected[index].OneWay, actual[index].OneWay);
            Assert.Equal(expected[index].SplineType, actual[index].SplineType);
            Assert.Equal(expected[index].Sports, actual[index].Sports);
            Assert.Equivalent(expected[index].Path, actual[index].Path, strict: true);
            Assert.Empty(actual[index].Segments);
            Assert.Empty(actual[index].Styles);
        }
    }

    private sealed class TestData : TheoryData<string, string, string>
    {
        public TestData()
        {
            var sauceWorldsPath = @"Data\Sauce\worlds";
            var sauceWorldRoadPaths = Directory
                .EnumerateFiles(sauceWorldsPath,
                    "roads.json", new EnumerationOptions() { RecurseSubdirectories = true })
                .ToArray();

            Dictionary<string, string> worldIdToSauceWorldRoadPath = [];
            foreach (var sauceWorldRoadPath in sauceWorldRoadPaths)
            {
                var pathParts = sauceWorldRoadPath.Split(Path.DirectorySeparatorChar);

                var worldId = pathParts[^2]; // Adjust index as needed
                worldIdToSauceWorldRoadPath[worldId] = sauceWorldRoadPath;
            }

            var zwiftRoads = Directory.EnumerateFiles(@"Data\Zwift\Worlds", "road.xml",
                new EnumerationOptions() { RecurseSubdirectories = true }).ToArray();

            Dictionary<string, string> worldIdToZwiftWorldRoadPath = [];
            foreach (var zwiftRoadPath in zwiftRoads)
            {
                var pathParts = zwiftRoadPath.Split(Path.DirectorySeparatorChar);
                var worldPathId = pathParts[^2]; // Adjust index as needed
                var worldId = worldPathId[5..]; // Assuming worldId is in the format "worldX"

                worldIdToZwiftWorldRoadPath[worldId] = zwiftRoadPath;
            }


            foreach (var worldId in worldIdToSauceWorldRoadPath.Keys)
            {
                var zwiftRoadPath = worldIdToZwiftWorldRoadPath[worldId];
                var sauceRoadPath = worldIdToSauceWorldRoadPath[worldId];

                var roadStylePath = Path.Combine(Path.GetDirectoryName(zwiftRoadPath)!, "roadstyle.xml");
                Add(zwiftRoadPath, roadStylePath, sauceRoadPath);

            }
        }
    }
}
