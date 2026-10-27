using System.Xml;
using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Entities;

[XmlRoot(ElementName = "world")]
public sealed class ZwiftXmlObjectEntitiesWorldElement
{
    [XmlElement("entities")]
    public required ZwiftXmlObjectEntitiesElement Entities { get; init; }

    [XmlAnyElement]
    public XmlElement[] AdditionalElements { get; set; } = [];
}

public sealed class ZwiftXmlObjectEntitiesElement
{
    private static readonly XmlSerializer TimingArchSerializer = new(typeof(ZwiftXmlObjectEntitiesTimingArchElement));
    private IReadOnlyList<ZwiftXmlObjectEntitiesTimingArchElement>? _timingArches;

    [XmlAnyElement]
    public XmlElement[] EntityElements { get; set; } = [];

    [XmlIgnore]
    public IReadOnlyList<ZwiftXmlObjectEntitiesTimingArchElement> TimingArches =>
        _timingArches ??= EntityElements
            .Where(element =>
                element.LocalName == "ent" &&
                element.GetAttribute("type") == "ENTITY_TYPE_TIMINGARCH")
            .Select(DeserializeTimingArch)
            .ToArray();

    private static ZwiftXmlObjectEntitiesTimingArchElement DeserializeTimingArch(XmlElement element)
    {
        using var reader = new XmlNodeReader(element);
        return (ZwiftXmlObjectEntitiesTimingArchElement?)TimingArchSerializer.Deserialize(reader)
            ?? throw new InvalidDataException("Unable to deserialize timing-arch entity.");
    }
}
