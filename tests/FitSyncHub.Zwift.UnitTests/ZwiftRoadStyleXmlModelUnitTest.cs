using FitSyncHub.Zwift.Xml;
using FitSyncHub.Zwift.Xml.Models;

namespace FitSyncHub.Zwift.UnitTests;

public sealed class ZwiftRoadStyleXmlModelUnitTest
{
    [Theory]
    [InlineData("Data\\Zwift\\Worlds\\world1\\roadstyle.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world2\\roadstyle.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world3\\roadstyle.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world4\\roadstyle.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world5\\roadstyle.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world6\\roadstyle.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world7\\roadstyle.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world8\\roadstyle.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world9\\roadstyle.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world10\\roadstyle.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world11\\roadstyle.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world12\\roadstyle.xml")]
    [InlineData("Data\\Zwift\\Worlds\\world13\\roadstyle.xml")]
    public void DeserializesRoadStyleWorld(string roadFilePath)
    {
        ZwiftXmlObjectRoadStyleRoot roadStyle;
#pragma warning disable IDE0063 // Use simple 'using' statement
        using (var rootParser = new ZwiftXmlObjectRootParser<ZwiftXmlObjectRoadStyleRoot>())
        {
            roadStyle = rootParser.Parse(roadFilePath);
        }
#pragma warning restore IDE0063 // Use simple 'using' statement

        Assert.NotNull(roadStyle.Version);
        Assert.Equal(1u, roadStyle.Version.Value);
        Assert.NotNull(roadStyle.Roads);
        Assert.NotNull(roadStyle.Roads.Defaults);

        var defaults = roadStyle.Roads.Defaults;
        Assert.Equal(1078, defaults.RoadWidth);
        Assert.True(defaults.MaxRoadWidth >= defaults.RoadWidth);
        Assert.True(defaults.MinRoadWidth <= defaults.RoadWidth);
        Assert.False(string.IsNullOrWhiteSpace(defaults.Assets));

        Assert.NotEmpty(roadStyle.Roads.Segments);
        var firstSegment = roadStyle.Roads.Segments[0];
        Assert.False(string.IsNullOrWhiteSpace(firstSegment.Style));
        Assert.False(string.IsNullOrWhiteSpace(firstSegment.Texture));
        Assert.False(string.IsNullOrWhiteSpace(firstSegment.Normals));
        Assert.False(string.IsNullOrWhiteSpace(firstSegment.Sound));
        Assert.True(firstSegment.ShakeFrequency >= 0);
    }
}
