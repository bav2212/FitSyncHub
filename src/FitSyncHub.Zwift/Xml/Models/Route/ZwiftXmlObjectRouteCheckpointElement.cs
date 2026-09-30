using System.Xml;
using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Route;

[XmlRoot(ElementName = "checkpoint")]
public sealed class ZwiftXmlObjectRouteCheckpointElement
{
    [XmlElement(ElementName = "entry")]
    public List<ZwiftXmlObjectRouteEntryElement> Entries { get; set; } = [];
}

