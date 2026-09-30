using System.Xml;
using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Route;

[XmlRoot(ElementName = "leadincheckpoint")]
public sealed class ZwiftXmlObjectRouteLeadInCheckpointElement
{
    [XmlElement(ElementName = "entry")]
    public List<ZwiftXmlObjectRouteEntryElement> Entries { get; set; } = [];
}

