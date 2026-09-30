using System.Xml;
using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Route;

[XmlRoot(ElementName = "leveleditorinfo")]
public sealed class ZwiftXmlObjectRouteLevelEditorInfoElement
{
    [XmlElement(ElementName = "routenodes")]
    public ZwiftXmlObjectRouteRouteNodesElement? RouteNodes { get; set; }
}

[XmlRoot(ElementName = "routenodes")]
public sealed class ZwiftXmlObjectRouteRouteNodesElement
{
    [XmlElement(ElementName = "entry")]
    public List<ZwiftXmlObjectRouteRouteNodeElement> Entries { get; set; } = [];
}
[XmlRoot(ElementName = "entry")]
public sealed class ZwiftXmlObjectRouteRouteNodeElement
{
    [XmlAttribute(AttributeName = "road")]
    public uint Road { get; set; }
    [XmlAttribute(AttributeName = "time")]
    public double Time { get; set; }

    [XmlAttribute(AttributeName = "isLoopTarget")]
    public bool IsLoopTarget { get; set; }
}
