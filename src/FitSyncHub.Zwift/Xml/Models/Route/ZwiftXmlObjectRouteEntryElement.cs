using System.Xml;
using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Route;

[XmlRoot(ElementName = "entry")]
public sealed class ZwiftXmlObjectRouteEntryElement
{
    [XmlAttribute(AttributeName = "road")]
    public uint Road { get; set; }
    [XmlAttribute(AttributeName = "dir")]
    public bool Dir { get; set; }
    [XmlAttribute(AttributeName = "time")]
    public double Time { get; set; }
    [XmlAttribute(AttributeName = "x")]
    public double X { get; set; }
    [XmlAttribute(AttributeName = "y")]
    public double Y { get; set; }
    [XmlAttribute(AttributeName = "z")]
    public double Z { get; set; }
}

