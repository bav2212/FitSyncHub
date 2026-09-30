using System.Xml;
using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Route;

[XmlRoot(ElementName = "highrescheckpoint")]
public sealed class ZwiftXmlObjectRouteHighResCheckpointElement
{
    [XmlElement(ElementName = "entry")]
    public List<ZwiftXmlObjectRouteEntryElement> Entries { get; set; } = [];
}

