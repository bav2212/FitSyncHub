using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Route;

[XmlRoot(ElementName = "VERSION")]
public sealed class ZwiftXmlObjectRouteVersionElement
{
    [XmlText]
    public uint Value { get; set; }
}
