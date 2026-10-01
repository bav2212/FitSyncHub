using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.RoadStyle;

[XmlRoot("roads")]
public sealed class ZwiftXmlObjectRoadStyleRoadsElement
{
    [XmlElement("defaults")]
    public ZwiftXmlObjectRoadStyleDefaultsElement? Defaults { get; set; }

    [XmlElement("segment")]
    public List<ZwiftXmlObjectRoadStyleSegmentElement> Segments { get; set; } = [];
}

public sealed class ZwiftXmlObjectRoadStyleDefaultsElement
{
    [XmlAttribute("roadwidth")]
    public double RoadWidth { get; set; }

    [XmlAttribute("shoulderwidth")]
    public double ShoulderWidth { get; set; }

    [XmlAttribute("min_roadwidth")]
    public double MinRoadWidth { get; set; }

    [XmlAttribute("max_roadwidth")]
    public double MaxRoadWidth { get; set; }

    [XmlAttribute("min_shoulderwidth")]
    public double MinShoulderWidth { get; set; }

    [XmlAttribute("max_shoulderwidth")]
    public double MaxShoulderWidth { get; set; }

    [XmlAttribute("road_u_width")]
    public double RoadUWidth { get; set; }

    [XmlAttribute("riderboundsratio")]
    public double RiderBoundsRatio { get; set; }

    [XmlAttribute("assets")]
    public string? Assets { get; set; }
}

public sealed class ZwiftXmlObjectRoadStyleSegmentElement
{
    [XmlAttribute("style")]
    public string? Style { get; set; }

    [XmlAttribute("texture")]
    public string? Texture { get; set; }

    [XmlAttribute("normals")]
    public string? Normals { get; set; }

    [XmlAttribute("sound")]
    public string? Sound { get; set; }

    [XmlAttribute("shakeMagStrong")]
    public double ShakeMagnitudeStrong { get; set; }

    [XmlAttribute("shakeMagWeak")]
    public double ShakeMagnitudeWeak { get; set; }

    [XmlAttribute("shakeFrequency")]
    public double ShakeFrequency { get; set; }

    [XmlAttribute("road_u_width")]
    public double RoadUWidth { get; set; }

    [XmlIgnore]
    public bool RoadUWidthSpecified { get; set; }

    [XmlAttribute("enableAmbientOcclusion")]
    public bool EnableAmbientOcclusion { get; set; }

    [XmlIgnore]
    public bool EnableAmbientOcclusionSpecified { get; set; }
}

