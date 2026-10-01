using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Road;

[XmlRoot(ElementName = "world")]
public sealed class ZwiftXmlObjectRoadWorldElement
{
    [XmlAttribute("version")]
    public uint Version { get; set; }

    [XmlElement("roadsettings")]
    public ZwiftXmlObjectRoadWorldRoadSettingsElement? RoadSettings { get; set; }

    [XmlArray("roads")]
    [XmlArrayItem("road")]
    public List<ZwiftXmlObjectRoadWorldRoadElement> Roads { get; set; } = [];
}
