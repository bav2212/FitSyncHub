using System.Xml;
using System.Xml.Serialization;
using FitSyncHub.Zwift.Xml.Abstractions;

namespace FitSyncHub.Zwift.Xml.Models;


public sealed class ZwiftXmlObjectClimbPortalRoadRoot : IZwiftXmlObjectRoot
{
    public required ZwiftXmlObjectClimbPortalRoadWorldElement World { get; init; }
}

[XmlRoot(ElementName = "world")]
public sealed class ZwiftXmlObjectClimbPortalRoadWorldElement
{
    [XmlArray("roads")]
    [XmlArrayItem("road")]
    public required List<ZwiftXmlObjectClimbPortalRoadWorldRoadElement> Roads { get; init; } = [];
}

public class ZwiftXmlObjectClimbPortalRoadWorldRoadElement
{
    [XmlElement("metadata")]
    public required ZwiftXmlObjectClimbPortalRoadWorldRoadMetadataElement Metadata { get; init; }
    [XmlElement("id")]
    public required int Id { get; init; }
    [XmlElement("splineType")]
    public required string SplineType { get; init; }
    [XmlElement("IsPortalRoad")]
    public required bool IsPortalRoad { get; init; }
    [XmlElement("physicSlopeOverride")]
    public required double PhysicSlopeOverride { get; init; }
    [XmlElement("min")]
    public required string Min { get; init; }
    [XmlElement("max")]
    public required string Max { get; init; }
    [XmlElement("centerRightRatio")]
    public required double CenterRightRatio { get; init; }
    [XmlElement("textureSegmentLength")]
    public required int TextureSegmentLength { get; init; }
    [XmlElement("looped")]
    public required int Looped { get; init; }
    [XmlElement("oneWay")]
    public required int OneWay { get; init; }
    [XmlElement("roadName")]
    public required string RoadName { get; init; }
    [XmlElement("defaultStyle")]
    public required int DefaultStyle { get; init; }
    [XmlElement("portalRoadEndGateRoadTime")]
    public required double PortalRoadEndGateRoadTime { get; init; }
    [XmlElement("isAvailable")]
    public required int IsAvailable { get; init; }
    [XmlElement("allowedSport")]
    public required int AllowedSport { get; init; }
    [XmlElement("snapToTesselation")]
    public required int SnapToTesselation { get; init; }
    [XmlElement("newTerrainAlign")]
    public required int NewTerrainAlign { get; init; }
    [XmlElement("riderBoundsRatio")]
    public required double RiderBoundsRatio { get; init; }
    [XmlAnyElement("portalRoadPartitions")]
    public required List<XmlElement> PortalRoadPartitions { get; init; }
    [XmlAnyElement("portalRoadLargePartitions")]
    public required List<XmlElement> PortalRoadLargePartitions { get; init; }
    [XmlAnyElement("ent")]
    public required List<XmlElement> Ent { get; init; }
}

public class ZwiftXmlObjectClimbPortalRoadWorldRoadMetadataElement
{
    [XmlElement("m_PortalRoadDescription")]
    public string? RoadDescription { get; init; }
    [XmlElement("m_PortalRoadInternalName")]
    public required string InternalName { get; init; }

    [XmlElement("m_PortalRoadUserFacingName")]
    public required string UserFacingName { get; init; }

    [XmlElement("m_PortalRoadPinIconPath")]
    public required string PinIconPath { get; init; }

    [XmlElement("m_PortalRoadMinimapIconPath")]
    public required string MinimapIconPath { get; init; }

    [XmlElement("m_PortalRoadHash")]
    public required long Hash { get; init; }

    [XmlElement("m_PortalRoadCourseLength")]
    public required double CourseLength { get; init; }

    [XmlElement("m_PortalRoadCourseAscentF")]
    public required double CourseAscentF { get; init; }

    [XmlElement("m_PortalRoadCourseAscentR")]
    public required double CourseAscentR { get; init; }

    [XmlElement("m_PortalRoadAverageSlope")]
    public required double AverageSlope { get; init; }

    [XmlElement("m_PortalRoadArchID")]
    public required int ArchId { get; init; }

    [XmlElement("m_PortalRoadArchFriendlyName")]
    public required string ArchFriendlyName { get; init; }

    [XmlElement("m_PortalRoadArchFemaleName")]
    public required string ArchFemaleName { get; init; }

    [XmlElement("m_PortalRoadMinCompletionTime")]
    public required double MinCompletionTime { get; init; }

    [XmlElement("m_PortalRoadJerseyIconPath")]
    public required string JerseyIconPath { get; init; }

    [XmlElement("m_PortalRoadJerseyPath")]
    public required string JerseyPath { get; init; }

    [XmlElement("m_PortalRoadJerseyPathFemale")]
    public required string JerseyPathFemale { get; init; }

    [XmlElement("m_PortalRoadLeaderJerseyName")]
    public required string LeaderJerseyName { get; init; }

    [XmlElement("m_PortalRoadJerseyHash")]
    public required uint JerseyHash { get; init; }

    [XmlElement("m_PortalRoadJerseyFemaleHash")]
    public required uint JerseyFemaleHash { get; init; }

    [XmlElement("m_PortalRoadEffort")]
    public required double Effort { get; init; }

    [XmlElement("m_PortalRoadDuration")]
    public required int Duration { get; init; }

    [XmlElement("m_PortalRoadExcludeFromDictionary")]
    public bool ExcludeFromDictionary { get; init; }
}
