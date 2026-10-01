using System.Xml;
using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Road;

public sealed class ZwiftXmlObjectRoadWorldRoadElement
{
    [XmlElement("id")]
    public int Id { get; set; }

    [XmlElement("splineType")]
    public string? SplineType { get; set; }

    [XmlElement("roadIsPaddock")]
    public bool? RoadIsPaddock { get; set; }

    [XmlElement("enableElevationDesaturation")]
    public bool? EnableElevationDesaturation { get; set; }

    [XmlElement("paddockExitRoadTime")]
    public double? PaddockExitRoadTime { get; set; }

    [XmlElement("physicSlopeOverride")]
    public double? PhysicSlopeOverride { get; set; }

    [XmlElement("min")]
    public string? Min { get; set; }

    [XmlElement("max")]
    public string? Max { get; set; }

    [XmlElement("centerRightRatio")]
    public double? CenterRightRatio { get; set; }

    [XmlElement("defaultRoadWidth")]
    public double? DefaultRoadWidth { get; set; }

    [XmlElement("textureSegmentLength")]
    public double? TextureSegmentLength { get; set; }

    [XmlElement("textureWidthMultiplier")]
    public double? TextureWidthMultiplier { get; set; }

    [XmlElement("looped")]
    public int? Looped { get; set; }

    [XmlElement("oneWay")]
    public int? OneWay { get; set; }

    [XmlElement("roadName")]
    public string? RoadName { get; set; }

    [XmlElement("defaultStyle")]
    public int? DefaultStyle { get; set; }

    [XmlElement("defaultStyleColor")]
    public uint? DefaultStyleColor { get; set; }

    [XmlElement("isAvailable")]
    public int? IsAvailable { get; set; }

    [XmlElement("allowedSport")]
    public int? AllowedSport { get; set; }

    [XmlElement("snapToTesselation")]
    public int? SnapToTesselation { get; set; }

    [XmlElement("newTerrainAlign")]
    public int? NewTerrainAlign { get; set; }

    [XmlElement("riderBoundsRatio")]
    public double? RiderBoundsRatio { get; set; }

    [XmlElement("ent")]
    public List<ZwiftXmlObjectRoadWorldRoadEntityElement> Entities { get; set; } = [];

    [XmlAnyElement]
    public XmlElement[] AdditionalElements { get; set; } = [];
}