using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Road;

public sealed class ZwiftXmlObjectRoadWorldRoadSettingsElement
{
    [XmlElement("rightTraffic")]
    public bool RightTraffic { get; set; }

    [XmlElement("roadVisualOffset")]
    public double RoadVisualOffset { get; set; }

    [XmlElement("physicsSlopeScale")]
    public double PhysicsSlopeScale { get; set; }

    [XmlElement("blurLength")]
    public int BlurLength { get; set; }
}
