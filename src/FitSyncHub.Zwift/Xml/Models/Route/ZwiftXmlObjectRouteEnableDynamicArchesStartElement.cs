using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Route;

[XmlRoot(ElementName = "enableDynamicArchesStart")]
public sealed class ZwiftXmlObjectRouteEnableDynamicArchesStartElement
{
    [XmlText]
    public bool Value { get; set; }
}
