using System.Xml;
using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Route;

[XmlRoot(ElementName = "private_spawn_area")]
public sealed class ZwiftXmlObjectRoutePrivateSpawnAreaElement
{
    [XmlAttribute(AttributeName = "road")]
    public uint Road { get; set; }
    [XmlAttribute(AttributeName = "forward")]
    public bool Forward { get; set; }
    [XmlAttribute(AttributeName = "starttime")]
    public double StartTime { get; set; }
    [XmlAttribute(AttributeName = "endtime")]
    public double EndTime { get; set; }
}

