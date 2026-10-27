using System.Text.Json;
using FitSyncHub.Zwift.Sauce;
using FitSyncHub.Zwift.Xml;
using FitSyncHub.Zwift.Xml.Models;

namespace FitSyncHub.Zwift.UnitTests;

public sealed class ZwiftSauceSegmentsConverterUnitTest
{
    [Theory]
    [MemberData(nameof(ZwiftSauceConverterTestData.Segments), MemberType = typeof(ZwiftSauceConverterTestData))]
    public void WorkCorrectly(
        uint worldId, string zwiftWorldEntitiesPath, string expectedSauceSegmentsPath)
    {
        using var entitiesParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectEntitiesRoot>();
        var parsedEntities = entitiesParser.Parse(zwiftWorldEntitiesPath);

        var actual = ZwiftSauceSegmentsConverter.Convert(worldId, parsedEntities);

        var sauceSegmentsJson = File.ReadAllText(expectedSauceSegmentsPath);
        var expected = JsonSerializer.Deserialize<ZwiftSauceSegment[]>(
            sauceSegmentsJson,
            ZwiftSauceConverterHelper.JsonSerializerOptions)!;

        Assert.Equal(expected.Length, actual.Length);
        Assert.Equivalent(expected, actual);
    }

}
