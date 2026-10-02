using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Xml;
using System.Xml.Linq;
using FitSyncHub.Zwift.Sauce;
using FitSyncHub.Zwift.Xml;
using FitSyncHub.Zwift.Xml.Models;

namespace FitSyncHub.Zwift.UnitTests;

public sealed class ZwiftSauceRouteConverterUnitTest
{
    [Theory]
    [ClassData(typeof(TestData))]
    public async Task ManifestConversion_WorkCorrectly(
        uint worldId, string xmlPath, ZwiftSauceRouteManifest[] expectedManifest)
    {
        using var rootParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectRouteRoot>();
        var zwiftInGameRoot = rootParser.Parse(xmlPath);

        var actual = ZwiftSauceRouteConverter
            .Convert(zwiftInGameRoot, worldId)
            .Manifest;

        Assert.Equivalent(expectedManifest, actual);
    }

    private sealed class TestData : TheoryData<uint, string, ZwiftSauceRouteManifest[]>
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

        public TestData()
        {
            var routesJsonPath = @"Data\Sauce\routes.json";
            var jsonRaw = File.ReadAllText(routesJsonPath);

            var rootElement = JsonDocument.Parse(jsonRaw);

            Dictionary<long, ZwiftSauceRouteManifest[]> sauceRouteManifests = [];
            foreach (var routeItem in rootElement.RootElement.EnumerateArray())
            {
                var routeId = routeItem.GetProperty("id").GetInt64();
                var expectedRouteManifests = routeItem.GetProperty("manifest").Deserialize<ZwiftSauceRouteManifest[]>(_jsonSerializerOptions)!;

                sauceRouteManifests[routeId] = expectedRouteManifests;
            }

            var zwiftRoutePaths = Directory.EnumerateFiles(@"Data\Zwift\Worlds", "routes*.xml",
                new EnumerationOptions() { RecurseSubdirectories = true }).ToArray();

            if (sauceRouteManifests.Count != zwiftRoutePaths.Length)
            {
                throw new InvalidOperationException($"The number of route manifests ({sauceRouteManifests.Count}) does not match the number of route XML files ({zwiftRoutePaths.Length}).");
            }

            foreach (var zwiftRoutefilePath in zwiftRoutePaths)
            {
                var pathParts = zwiftRoutefilePath.Split(Path.DirectorySeparatorChar, Path.DirectorySeparatorChar);
                var worldId = uint.Parse(pathParts[^3][5..]);

                using var reader = XmlReader.Create(zwiftRoutefilePath, new XmlReaderSettings
                {
                    IgnoreComments = true,
                    IgnoreWhitespace = true,
                    // THIS is the important part: allow multiple top-level elements
                    ConformanceLevel = ConformanceLevel.Fragment,
                });

                long routeId = 0;
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }

                    var xmlElementName = reader.Name;
                    if (xmlElementName == "route")
                    {
                        var routeElement = XNode.ReadFrom(reader) as XElement;
                        var nameHash = routeElement!.Attribute("nameHash")!.Value;
                        routeId = long.Parse(nameHash);
                        break;
                    }
                }

                Add(worldId, zwiftRoutefilePath, sauceRouteManifests[routeId]);
            }
        }
    }
}
