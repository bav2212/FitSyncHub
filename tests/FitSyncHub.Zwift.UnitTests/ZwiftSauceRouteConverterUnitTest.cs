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
        string xmlPath, ZwiftSauceRouteManifest[] expectedManifest)
    {
        using var rootParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectRouteRoot>();
        var zwiftInGameRoot = rootParser.Parse(xmlPath);

        var actual = ZwiftSauceRouteConverter
            .Convert(zwiftInGameRoot, /*don't care here'*/ worldId: 1)
            .Manifest;

        Assert.Equivalent(expectedManifest, actual);
    }

    private sealed class TestData : TheoryData<string, ZwiftSauceRouteManifest[]>
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

            Dictionary<long, ZwiftSauceRouteManifest[]> routeManifests = [];
            foreach (var routeItem in rootElement.RootElement.EnumerateArray())
            {
                var routeId = routeItem.GetProperty("id").GetInt64();
                var expectedRouteManifests = routeItem.GetProperty("manifest").Deserialize<ZwiftSauceRouteManifest[]>(_jsonSerializerOptions)!;

                routeManifests[routeId] = expectedRouteManifests;
            }

            var routes = Directory.EnumerateFiles(@"Data\Zwift\Worlds", "routes*.xml",
                new EnumerationOptions() { RecurseSubdirectories = true }).ToArray();

            if (routeManifests.Count != routes.Length)
            {
                throw new InvalidOperationException($"The number of route manifests ({routeManifests.Count}) does not match the number of route XML files ({routes.Length}).");
            }

            foreach (var filePath in routes)
            {
                using var reader = XmlReader.Create(filePath, new XmlReaderSettings
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

                Add(filePath, routeManifests[routeId]);
            }
        }
    }
}
