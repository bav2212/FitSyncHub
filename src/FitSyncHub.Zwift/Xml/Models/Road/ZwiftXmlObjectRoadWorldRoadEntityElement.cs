using System.Xml;
using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Road;

public sealed class ZwiftXmlObjectRoadWorldRoadEntityElement
{
    [XmlAttribute("type")]
    public string? Type { get; set; }

    [XmlAttribute("m_pos")]
    public string? Position { get; set; }

    [XmlAttribute("m_Name")]
    public string? Name { get; set; }

    [XmlAttribute("m_roadTime1")]
    public double RoadTime1 { get; set; }

    [XmlIgnore]
    public bool RoadTime1Specified { get; set; }

    [XmlAttribute("m_roadTime2")]
    public double RoadTime2 { get; set; }

    [XmlIgnore]
    public bool RoadTime2Specified { get; set; }

    [XmlAttribute("m_roadId")]
    public int RoadId { get; set; }

    [XmlAttribute("m_markerId")]
    public int MarkerId { get; set; }

    [XmlAttribute("m_roadWidth")]
    public double RoadWidth { get; set; }

    [XmlAttribute("m_shoulderWidth")]
    public double ShoulderWidth { get; set; }

    [XmlAttribute("m_FlattenWidth")]
    public double FlattenWidth { get; set; }

    [XmlAttribute("m_VisualOffset")]
    public double VisualOffset { get; set; }

    [XmlAttribute("m_TiltAngle")]
    public double TiltAngle { get; set; }

    [XmlAttribute("m_tangentIn")]
    public string? TangentIn { get; set; }

    [XmlAttribute("m_tangentOut")]
    public string? TangentOut { get; set; }

    [XmlAttribute("m_tangentInAuto")]
    public int TangentInAuto { get; set; }

    [XmlAttribute("m_bAggresiveBlur")]
    public bool AggresiveBlur { get; set; }

    [XmlAttribute("m_bAutoSnap")]
    public bool AutoSnap { get; set; }

    [XmlAttribute("m_bDecoupledFromTerrain")]
    public bool DecoupledFromTerrain { get; set; }

    [XmlAttribute("m_bStraight")]
    public bool Straight { get; set; }

    [XmlAttribute("m_bIgnoreRoadWidth")]
    public bool IgnoreRoadWidth { get; set; }

    [XmlAttribute("m_bDisplayWhenLocked")]
    public bool DisplayWhenLocked { get; set; }

    [XmlAttribute("m_isInvisible")]
    public bool IsInvisible { get; set; }

    [XmlAttribute("m_applyWidth")]
    public int ApplyWidth { get; set; }

    [XmlAttribute("m_style")]
    public int Style { get; set; }

    [XmlIgnore]
    public bool StyleSpecified { get; set; }

    [XmlAttribute("m_smoothRatio0")]
    public double SmoothRatio0 { get; set; }

    [XmlAttribute("m_smoothRatio1")]
    public double SmoothRatio1 { get; set; }

    [XmlAttribute("m_textureScale")]
    public double TextureScale { get; set; }

    [XmlAttribute("m_textureUScale")]
    public double TextureUScale { get; set; }

    [XmlAttribute("m_audioVariableName")]
    public string? AudioVariableName { get; set; }

    [XmlAttribute("zgID")]
    public string? ZgId { get; set; }

    [XmlElement("forward")]
    public ZwiftXmlObjectRoadWorldRoadEntityDirectionElement? Forward { get; set; }

    [XmlElement("reverse")]
    public ZwiftXmlObjectRoadWorldRoadEntityDirectionElement? Reverse { get; set; }

    [XmlAnyAttribute]
    public XmlAttribute[] AdditionalAttributes { get; set; } = [];

    [XmlAnyElement]
    public XmlElement[] AdditionalElements { get; set; } = [];
}

public sealed class ZwiftXmlObjectRoadWorldRoadEntityDirectionElement
{
    [XmlElement("option")]
    public List<string> Options { get; set; } = [];
}