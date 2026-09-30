using System.Xml;
using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Route;

[XmlRoot(ElementName = "leadinhighrescheckpoint")]
public sealed class ZwiftXmlObjectRouteLeadInHighResCheckpointElement
{
    [XmlElement(ElementName = "entry")]
    public List<ZwiftXmlObjectRouteEntryElement> Entries { get; set; } = [];
}

