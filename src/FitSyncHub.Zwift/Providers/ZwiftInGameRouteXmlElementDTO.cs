using System.Xml;
using System.Xml.Serialization;

namespace FitSyncHub.Zwift.Providers;

public record ZwiftInGameRootXmlObject
{
    public required ZwiftInGameRouteXmlElementDTO Route { get; init; }
    public required ZwiftInGameHomedataXmlElementDTO Homedata { get; init; }
}

[XmlRoot(ElementName = "homedata")]
public sealed class ZwiftInGameHomedataXmlElementDTO
{

    [XmlAttribute(AttributeName = "difficulty")]
    public double Difficulty { get; set; }
    [XmlAttribute(AttributeName = "duration")]
    public int Duration { get; set; }
    [XmlAttribute(AttributeName = "xp")]
    public uint Xp { get; set; }
    [XmlAttribute(AttributeName = "bikeRec")]
    public bool BikeRec { get; set; }
    [XmlAttribute(AttributeName = "workoutRec")]
    public bool WorkoutRec { get; set; }
    [XmlAttribute(AttributeName = "bikeType")]
    public uint BikeType { get; set; }
    [XmlAttribute(AttributeName = "runRec")]
    public bool RunRec { get; set; }
    [XmlAttribute(AttributeName = "publishedOn")]
    public string? PublishedOn { get; set; }
    [XmlAttribute(AttributeName = "rowRec")]
    public bool RowRec { get; set; }
}

[XmlRoot(ElementName = "route")]
public sealed class ZwiftInGameRouteXmlElementDTO
{
    [XmlAttribute("name")]
    public required string Name
    {
        get;
        set => field = value!.Trim();
    }

    [XmlAttribute("eventPaddocks")]
    public string? EventPaddocksRaw { get; set; }

    [XmlIgnore]
    public int[] EventPaddocksArray => [.. (EventPaddocksRaw ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)];

    [XmlAttribute("gameDictName")]
    public string? GameDictName { get; set; }
    [XmlAttribute("nameHash")]
    public uint NameHash { get; set; }
    [XmlAttribute("locKey")]
    public string LocKey { get; set; } = null!;
    [XmlAttribute("distanceInMeters")]
    public double DistanceInMeters { get; set; }
    [XmlAttribute("ascentInMeters")]
    public double AscentInMeters { get; set; }
    [XmlAttribute("leadinDistanceInMeters")]
    public double LeadInDistanceInMeters { get; set; }
    [XmlAttribute("leadinAscentInMeters")]
    public double LeadInAscentInMeters { get; set; }
    [XmlAttribute("freeRideLeadinDistanceInMeters")]
    public double FreeRideLeadinDistanceInMeters { get; set; }
    [XmlAttribute("freeRideLeadinAscentInMeters")]
    public double FreeRideLeadinAscentInMeters { get; set; }
    [XmlAttribute("meetupLeadinDistanceInMeters")]
    public double MeetupLeadinDistanceInMeters { get; set; }
    [XmlAttribute("meetupLeadinAscentInMeters")]
    public double MeetupLeadinAscentInMeters { get; set; }
    [XmlAttribute("distanceBetweenFirstLastLrCPsInMeters")]
    public double DistanceBetweenFirstLastLrCPsInMeters { get; set; }
    [XmlAttribute("levelLocked")]
    public int LevelLocked { get; set; }
    [XmlAttribute("mapID")]
    public int MapID { get; set; }
    [XmlAttribute("supportedLaps")]
    public bool SupportedLaps { get; set; }
    [XmlAttribute("approvedForProgressMinimap")]
    public bool ApprovedForProgressMinimap { get; set; }
    [XmlAttribute("reverseRouteName")]
    public string? ReverseRouteName { get; set; }
    [XmlAttribute("sportType")]
    public int SportType { get; set; }
    [XmlAttribute("hasMatchingLeaderboardSegment")]
    public bool HasMatchingLeaderboardSegment { get; set; }
    [XmlAttribute("ascentBetweenFirstLastLrCPsInMeters")]
    public double AscentBetweenFirstLastLrCPsInMeters { get; set; }
    [XmlAttribute("defaultLeadinDistanceInMeters")]
    public double DefaultLeadinDistanceInMeters { get; set; }
    [XmlAttribute("defaultLeadinAscentInMeters")]
    public double DefaultLeadinAscentInMeters { get; set; }
    [XmlAttribute("eventOnly")]
    public bool EventOnly { get; set; }
    [XmlAttribute("useAlternateEventRamp")]
    public int UseAlternateEventRamp { get; set; }
    [XmlAttribute("hasPortalRoad")]
    public bool HasPortalRoad { get; set; }
    [XmlAttribute("excludeFromGameDictionary")]
    public bool ExcludeFromGameDictionary { get; set; }
    [XmlAttribute("blockedForMeetups")]
    public bool BlockedForMeetups { get; set; }
    [XmlAttribute("blockedForClubs")]
    public bool BlockedForClubs { get; set; }
    [XmlAttribute("zwiftEventOnly")]
    public bool ZwiftEventOnly { get; set; }
    [XmlAttribute("supportsTimeTrialMode")]
    public bool SupportsTimeTrialMode { get; set; }
    [XmlAttribute("showProgressInHUD")]
    public string? ShowProgressInHUD { get; set; }
    [XmlAttribute("routeEndsUnderTimingArch")]
    public bool RouteEndsUnderTimingArch { get; set; }
    [XmlAttribute("scentBetweenFirstLastLrCPsInMeters")]
    public double ScentBetweenFirstLastLrCPsInMeters { get; set; }
    [XmlAttribute("normalDecisionCount")]
    public int NormalDecisionCount { get; set; }
    [XmlAttribute("checkPointCount")]
    public int CheckPointCount { get; set; }
    [XmlAttribute("routeName")]
    public string? RouteName { get; set; }
    [XmlAttribute("blockedForTimeTrial")]
    public bool BlockedForTimeTrial { get; set; }
}
