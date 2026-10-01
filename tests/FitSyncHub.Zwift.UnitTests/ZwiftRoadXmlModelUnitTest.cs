using FitSyncHub.Zwift.Xml;
using FitSyncHub.Zwift.Xml.Models;
using FitSyncHub.Zwift.Xml.Models.Road;

namespace FitSyncHub.Zwift.UnitTests;

public sealed class ZwiftRoadXmlModelUnitTest
{
    [Theory]
    [InlineData("Data\\Zwift\\Worlds\\world1\\road.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world2\\road.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world3\\road.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world4\\road.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world5\\road.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world6\\road.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world7\\road.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world8\\road.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world9\\road.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world10\\road.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world11\\road.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world12\\road.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world13\\road.xml")]
    public void DeserializesRoadWorld(string roadFilePath)
    {
        ZwiftXmlObjectRoadWorldElement roadWorld;
        using (var rootParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectRoadRoot>())
        {
            roadWorld = rootParser.Parse(roadFilePath).World;
        }

        Assert.Equal(1u, roadWorld.Version);
        Assert.NotNull(roadWorld.RoadSettings);
        Assert.NotEmpty(roadWorld.Roads);
        Assert.Contains(roadWorld.Roads, road => road.Entities.Count > 0);
    }
}
