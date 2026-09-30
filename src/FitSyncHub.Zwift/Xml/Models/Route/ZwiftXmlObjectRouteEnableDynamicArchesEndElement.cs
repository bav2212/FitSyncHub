using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Route;

[XmlRoot(ElementName = "enableDynamicArchesEnd")]
public sealed class ZwiftXmlObjectRouteEnableDynamicArchesEndElement
{
    [XmlText]
    public bool Value { get; set; }
}
