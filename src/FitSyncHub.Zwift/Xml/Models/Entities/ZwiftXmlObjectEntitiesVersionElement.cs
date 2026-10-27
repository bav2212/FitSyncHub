using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Entities;

[XmlRoot(ElementName = "VERSION")]
public sealed class ZwiftXmlObjectEntitiesVersionElement
{
    [XmlText]
    public uint Value { get; set; }
}
