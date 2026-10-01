using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.RoadStyle;

[XmlRoot("VERSION")]
public sealed class ZwiftXmlObjectRoadStyleVersionElement
{
    [XmlText]
    public uint Value { get; set; }
}
