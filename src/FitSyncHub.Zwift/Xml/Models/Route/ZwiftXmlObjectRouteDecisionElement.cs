using System.Xml;
using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Route;

[XmlRoot(ElementName = "decision")]
public sealed class ZwiftXmlObjectRouteDecisionElement
{
    [XmlElement(ElementName = "normal")]
    public ZwiftXmlObjectRouteNormalElement? Normal { get; set; }
}

[XmlRoot(ElementName = "normal")]
public sealed class ZwiftXmlObjectRouteNormalElement
{
    [XmlAttribute(AttributeName = "infiniteLoopStart")]
    public int InfiniteLoopStart { get; set; }
    [XmlAttribute(AttributeName = "saStart")]
    public int SaStart { get; set; }
    [XmlAttribute(AttributeName = "psaStart")]
    public int PsaStart { get; set; }
    [XmlElement(ElementName = "entry")]
    public List<ZwiftXmlObjectRouteDecisionEntryElement> Entries { get; set; } = [];
}

[XmlRoot(ElementName = "entry")]
public sealed class ZwiftXmlObjectRouteDecisionEntryElement
{
    [XmlAttribute(AttributeName = "markerId")]
    public uint MarkerId { get; set; }
    [XmlAttribute(AttributeName = "turn")]
    // TODO, change to enum. Sometimes it's number, sometimes it's string. 
    public string? Turn { get; set; }
    [XmlAttribute(AttributeName = "forward")]
    public bool Forward { get; set; }
    [XmlAttribute(AttributeName = "stub")]
    public bool Stub { get; set; }
}

