using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Xml.Models.Entities;

[XmlRoot(ElementName = "ent")]
public sealed class ZwiftXmlObjectEntitiesTimingArchElement
{
    [XmlAttribute("type")]
    public string? Type { get; set; }

    [XmlAttribute("zgID")]
    public string? ZgId { get; set; }

    [XmlAttribute("m_telemetryArchType")]
    public int TelemetryArchType { get; set; }
    [XmlIgnore]
    public bool TelemetryArchTypeSpecified { get; set; }

    [XmlAttribute("m_Name")]
    public string? Name { get; set; }

    [XmlAttribute("m_roadId")]
    public int RoadId { get; set; }
    [XmlIgnore]
    public bool RoadIdSpecified { get; set; }

    [XmlAttribute("m_roadTime")]
    public double RoadTime { get; set; }
    [XmlIgnore]
    public bool RoadTimeSpecified { get; set; }

    [XmlAttribute("m_archGdePath")]
    public string? ArchGdePath { get; set; }

    [XmlAttribute("m_startLineSplineTimeF")]
    public double StartLineSplineTimeForward { get; set; }
    [XmlIgnore]
    public bool StartLineSplineTimeForwardSpecified { get; set; }

    [XmlAttribute("m_beginTimerSplineTimeF")]
    public double BeginTimerSplineTimeForward { get; set; }
    [XmlIgnore]
    public bool BeginTimerSplineTimeForwardSpecified { get; set; }

    [XmlAttribute("m_startLineSplineTimeR")]
    public double StartLineSplineTimeReverse { get; set; }
    [XmlIgnore]
    public bool StartLineSplineTimeReverseSpecified { get; set; }

    [XmlAttribute("m_beginTimerSplineTimeR")]
    public double BeginTimerSplineTimeReverse { get; set; }
    [XmlIgnore]
    public bool BeginTimerSplineTimeReverseSpecified { get; set; }

    [XmlAttribute("m_minTimeF")]
    public double MinimumTimeForward { get; set; }
    [XmlIgnore]
    public bool MinimumTimeForwardSpecified { get; set; }

    [XmlAttribute("m_minTimeR")]
    public double MinimumTimeReverse { get; set; }
    [XmlIgnore]
    public bool MinimumTimeReverseSpecified { get; set; }

    [XmlAttribute("m_ArchColor")]
    public uint ArchColor { get; set; }
    [XmlIgnore]
    public bool ArchColorSpecified { get; set; }

    [XmlAttribute("m_ArchIconPath")]
    public string? ArchIconPath { get; set; }

    [XmlAttribute("m_ArchFriendlyName")]
    public string? ArchFriendlyName { get; set; }

    [XmlAttribute("m_ArchFriendlyFemaleName")]
    public string? ArchFriendlyFemaleName { get; set; }

    [XmlAttribute("m_ArchFriendlyNameR")]
    public string? ArchFriendlyNameReverse { get; set; }

    [XmlAttribute("m_ArchFriendlyFemaleNameR")]
    public string? ArchFriendlyFemaleNameReverse { get; set; }

    [XmlAttribute("m_ArchSegmentAvgInclineF")]
    public double ArchSegmentAverageInclineForward { get; set; }
    [XmlIgnore]
    public bool ArchSegmentAverageInclineForwardSpecified { get; set; }

    [XmlAttribute("m_ArchSegmentAvgInclineR")]
    public double ArchSegmentAverageInclineReverse { get; set; }
    [XmlIgnore]
    public bool ArchSegmentAverageInclineReverseSpecified { get; set; }

    [XmlAttribute("m_ArchSegmentDistanceF")]
    public double ArchSegmentDistanceForward { get; set; }
    [XmlIgnore]
    public bool ArchSegmentDistanceForwardSpecified { get; set; }

    [XmlAttribute("m_ArchSegmentDistanceR")]
    public double ArchSegmentDistanceReverse { get; set; }
    [XmlIgnore]
    public bool ArchSegmentDistanceReverseSpecified { get; set; }

    [XmlAttribute("m_ArchSegmentTotalInclineInMetersF")]
    public double ArchSegmentTotalInclineInMetersForward { get; set; }
    [XmlIgnore]
    public bool ArchSegmentTotalInclineInMetersForwardSpecified { get; set; }

    [XmlAttribute("m_ArchSegmentTotalInclineInMetersR")]
    public double ArchSegmentTotalInclineInMetersReverse { get; set; }
    [XmlIgnore]
    public bool ArchSegmentTotalInclineInMetersReverseSpecified { get; set; }

    [XmlAttribute("m_ArchId")]
    public int ArchId { get; set; }
    [XmlIgnore]
    public bool ArchIdSpecified { get; set; }

    [XmlAttribute("m_ArchIconFemalePath")]
    public string? ArchIconFemalePath { get; set; }

    [XmlAttribute("m_ArchAlertTime")]
    public double ArchAlertTime { get; set; }
    [XmlIgnore]
    public bool ArchAlertTimeSpecified { get; set; }

    [XmlAttribute("m_jerseyIconPath")]
    public string? JerseyIconPath { get; set; }

    [XmlAttribute("m_jerseyPath")]
    public string? JerseyPath { get; set; }

    [XmlAttribute("m_jerseyPath_Female")]
    public string? JerseyPathFemale { get; set; }

    [XmlAttribute("m_leaderJersey_Name")]
    public string? LeaderJerseyName { get; set; }

    [XmlAttribute("m_pinIconPath")]
    public string? PinIconPath { get; set; }

    [XmlAttribute("m_minimapIconPath")]
    public string? MinimapIconPath { get; set; }

    [XmlAttribute("m_LayerName")]
    public string? LayerName { get; set; }

    [XmlAttribute("m_bDontGenerateHashes")]
    public bool DontGenerateHashes { get; set; }
    [XmlIgnore]
    public bool DontGenerateHashesSpecified { get; set; }

    [XmlAttribute("m_overrideNetworkHashF")]
    public long OverrideNetworkHashForward { get; set; }
    [XmlIgnore]
    public bool OverrideNetworkHashForwardSpecified { get; set; }

    [XmlAttribute("m_overrideNetworkHashR")]
    public long OverrideNetworkHashReverse { get; set; }
    [XmlIgnore]
    public bool OverrideNetworkHashReverseSpecified { get; set; }

    [XmlAttribute("m_bEnableForward")]
    public bool EnableForward { get; set; }
    [XmlIgnore]
    public bool EnableForwardSpecified { get; set; }

    [XmlAttribute("m_bEnableBackward")]
    public bool EnableBackward { get; set; }
    [XmlIgnore]
    public bool EnableBackwardSpecified { get; set; }

    [XmlAttribute("m_bShowLeaderboard")]
    public bool ShowLeaderboard { get; set; }
    [XmlIgnore]
    public bool ShowLeaderboardSpecified { get; set; }

    [XmlAttribute("m_bGivesPowerUp")]
    public bool GivesPowerUp { get; set; }
    [XmlIgnore]
    public bool GivesPowerUpSpecified { get; set; }

    [XmlAttribute("m_bSetTimeFromDistance")]
    public bool SetTimeFromDistance { get; set; }
    [XmlIgnore]
    public bool SetTimeFromDistanceSpecified { get; set; }

    [XmlAttribute("m_bEstimateFTP")]
    public bool EstimateFtp { get; set; }
    [XmlIgnore]
    public bool EstimateFtpSpecified { get; set; }

    [XmlAttribute("m_bTimeDistanceAutoUpdate")]
    public bool TimeDistanceAutoUpdate { get; set; }
    [XmlIgnore]
    public bool TimeDistanceAutoUpdateSpecified { get; set; }

    [XmlAttribute("m_bShowTimingHUD")]
    public bool ShowTimingHud { get; set; }
    [XmlIgnore]
    public bool ShowTimingHudSpecified { get; set; }

    [XmlAttribute("m_beginTimerDistanceFromStartF")]
    public double BeginTimerDistanceFromStartForward { get; set; }
    [XmlIgnore]
    public bool BeginTimerDistanceFromStartForwardSpecified { get; set; }

    [XmlAttribute("m_bStopwatchRequiresAllCheckpoints")]
    public bool StopwatchRequiresAllCheckpoints { get; set; }
    [XmlIgnore]
    public bool StopwatchRequiresAllCheckpointsSpecified { get; set; }

    [XmlAttribute("m_fMarkerVisualOffset")]
    public double MarkerVisualOffset { get; set; }
    [XmlIgnore]
    public bool MarkerVisualOffsetSpecified { get; set; }

    [XmlAttribute("m_scale")]
    public string? Scale { get; set; }

    [XmlAttribute("m_rot")]
    public string? Rotation { get; set; }

    [XmlAttribute("m_tileBadgePath")]
    public string? TileBadgePath { get; set; }
}
