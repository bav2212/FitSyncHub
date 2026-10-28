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
        var expected = DeserializeSegments(sauceSegmentsJson).ToArray();

        Assert.Equal(expected.Length, actual.Length);
        Assert.Equivalent(expected, actual);
    }

    private static IEnumerable<ZwiftSauceSegment> DeserializeSegments(string sauceSegmentsJson)
    {
        var jsonDocument = JsonDocument.Parse(sauceSegmentsJson);
        if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var arrayItem in jsonDocument.RootElement.EnumerateArray())
            {
                yield return arrayItem
                    .Deserialize<ZwiftSauceSegment>(ZwiftSauceConverterHelper.JsonSerializerOptions)!;
            }
            yield break;
        }

        // some file have only one segment and not wrapped with array
        yield return jsonDocument.RootElement
            .Deserialize<ZwiftSauceSegment>(ZwiftSauceConverterHelper.JsonSerializerOptions)!;
    }
}
