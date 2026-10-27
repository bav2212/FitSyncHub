using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FitSyncHub.Zwift.Sauce;
using FitSyncHub.Zwift.Xml;
using FitSyncHub.Zwift.Xml.Models;

namespace FitSyncHub.Zwift.UnitTests;

public sealed class ZwiftSauceSegmentsConverterUnitTest
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
        uint worldId, string zwiftWorldEntitiesPath, string expectedSauceSegmentsPath)
    {
        using var entitiesParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectEntitiesRoot>();
        var parsedEntities = entitiesParser.Parse(zwiftWorldEntitiesPath);

        var actual = ZwiftSauceSegmentsConverter.Convert(worldId, parsedEntities);

        var sauceSegmentsJson = File.ReadAllText(expectedSauceSegmentsPath);
        var expected = JsonSerializer.Deserialize<ZwiftSauceSegment[]>(sauceSegmentsJson, _jsonSerializerOptions)!;

        Assert.Equal(expected.Length, actual.Length);
        Assert.Equivalent(expected, actual);
    }

    private sealed class TestData : TheoryData<uint, string, string>
    {
        public TestData()
        {
            var sauceWorldsPath = @"Data\Sauce\worlds";
            var sauceWorldSegmentsPaths = Directory
                .EnumerateFiles(sauceWorldsPath, "segments.json", new EnumerationOptions() { RecurseSubdirectories = true })
                .ToArray();

            Dictionary<uint, string> worldIdToSauceWorldSegmentsPath = [];
            foreach (var sauceWorldSegmentsPath in sauceWorldSegmentsPaths)
            {
                var pathParts = sauceWorldSegmentsPath.Split(Path.DirectorySeparatorChar);

                var worldId = uint.Parse(pathParts[^2]);
                worldIdToSauceWorldSegmentsPath[worldId] = sauceWorldSegmentsPath;
            }

            var zwiftEntitiesPaths = Directory
                .EnumerateFiles(@"Data\Zwift\Worlds", "entities.xml", new EnumerationOptions() { RecurseSubdirectories = true })
                .ToArray();

            Dictionary<uint, string> worldIdToZwiftWorldEntitiesPath = [];
            foreach (var zwiftEntitiesPath in zwiftEntitiesPaths)
            {
                var pathParts = zwiftEntitiesPath.Split(Path.DirectorySeparatorChar);
                var worldPathId = pathParts[^2]; // Adjust index as needed
                var worldId = uint.Parse(worldPathId[5..]); // Assuming worldId is in the format "worldX"

                worldIdToZwiftWorldEntitiesPath[worldId] = zwiftEntitiesPath;
            }

            foreach (var worldId in worldIdToSauceWorldSegmentsPath.Keys)
            {
                var zwiftEntititesPath = worldIdToZwiftWorldEntitiesPath[worldId];
                var sauceSegmentsPath = worldIdToSauceWorldSegmentsPath[worldId];

                Add(worldId, zwiftEntititesPath, sauceSegmentsPath);
            }
        }
    }
}
