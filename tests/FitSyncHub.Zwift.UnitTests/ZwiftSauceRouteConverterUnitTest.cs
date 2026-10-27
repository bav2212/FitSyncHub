using System.Text.Json;
using FitSyncHub.Zwift.Sauce;
using FitSyncHub.Zwift.Xml;
using FitSyncHub.Zwift.Xml.Models;

namespace FitSyncHub.Zwift.UnitTests;

public sealed class ZwiftSauceRouteConverterUnitTest
{
    [Theory]
    [MemberData(nameof(ZwiftSauceConverterTestData.Routes), MemberType = typeof(ZwiftSauceConverterTestData))]
    public async Task ManifestConversion_WorkCorrectly(
        uint worldId, string xmlPath, string expectedManifestJson)
    {
        using var rootParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectRouteRoot>();
        var zwiftInGameRoot = rootParser.Parse(xmlPath);

        var actual = ZwiftSauceRouteConverter
            .Convert(zwiftInGameRoot, worldId)
            .Manifest;

        var expectedManifest = JsonSerializer.Deserialize<ZwiftSauceRouteManifest[]>(
            expectedManifestJson,
            ZwiftSauceConverterHelper.JsonSerializerOptions);

        Assert.Equivalent(expectedManifest, actual);
    }
}
