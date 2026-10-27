using FitSyncHub.Zwift.Xml;
using FitSyncHub.Zwift.Xml.Models;

namespace FitSyncHub.Zwift.UnitTests;

public sealed class ZwiftEntitiesXmlModelUnitTest
{
    [Fact]
    public void DeserializesEntitiesWorld()
    {
        using var rootParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectEntitiesRoot>();
        var entitiesRoot = rootParser.Parse(@"Data\Zwift\Worlds\world1\entities.xml");

        Assert.Equal(0u, entitiesRoot.Version.Value);
        Assert.NotEmpty(entitiesRoot.World.Entities.EntityElements);

        var firstEntity = entitiesRoot.World.Entities.EntityElements[0];
        Assert.Equal("ENTITY_TYPE_REGIONMARKER", firstEntity.GetAttribute("type"));
        Assert.True(firstEntity.HasAttribute("m_pos"));
        Assert.Equal(
            entitiesRoot.World.Entities.EntityElements.Count(element =>
                element.GetAttribute("type") == "ENTITY_TYPE_TIMINGARCH"),
            entitiesRoot.World.Entities.TimingArches.Count);
        Assert.All(entitiesRoot.World.Entities.TimingArches, entity =>
            Assert.Equal("ENTITY_TYPE_TIMINGARCH", entity.Type));
    }

    [Fact]
    public void DeserializesTimingArchAttributesAsTypedValues()
    {
        using var rootParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectEntitiesRoot>();
        var entitiesRoot = rootParser.Parse(@"Data\Zwift\Worlds\world8\entities.xml");

        var timingArch = Assert.Single(entitiesRoot.World.Entities.TimingArches, entity =>
            entity.Name == "Crit City");

        Assert.Equal("ENTITY_TYPE_TIMINGARCH", timingArch.Type);
        Assert.Equal(4, timingArch.TelemetryArchType);
        Assert.Equal(0.6098652289, timingArch.RoadTime);
        Assert.Equal("data/Environment/Structures/TechArch_01/ZwiftLapArch.gde", timingArch.ArchGdePath);
        Assert.Equal(4282415103u, timingArch.ArchColor);
        Assert.Equal(90d, timingArch.MinimumTimeForward);
        Assert.True(timingArch.MinimumTimeForwardSpecified);
        Assert.True(timingArch.StopwatchRequiresAllCheckpoints);
        Assert.True(timingArch.StopwatchRequiresAllCheckpointsSpecified);
        Assert.True(timingArch.DontGenerateHashes);
        Assert.Equal(246752921L, timingArch.OverrideNetworkHashForward);
        Assert.Equal(-246752921L, timingArch.OverrideNetworkHashReverse);
    }
}
