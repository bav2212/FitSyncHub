using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Route;

[XmlRoot(ElementName = "segment_data")]
public sealed class ZwiftXmlObjectRouteSegmentDataElement
{
    [XmlElement(ElementName = "entry")]
    public List<ZwiftXmlObjectRouteSegmentDataEntryElement> Entries { get; set; } = [];
}

[XmlRoot(ElementName = "entry")]
public sealed class ZwiftXmlObjectRouteSegmentDataEntryElement
{
    [XmlAttribute(AttributeName = "hash")]
    public long Hash { get; set; }

    [XmlAttribute(AttributeName = "percentStart")]
    public double PercentStart { get; set; }

    [XmlAttribute(AttributeName = "percentEnd")]
    public double PercentEnd { get; set; }
}