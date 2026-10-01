using System.Text.Json.Serialization;

namespace FitSyncHub.Zwift.Sauce;

public sealed record ZwiftSauceRoute
{
    [JsonConverter(typeof(BoolAsNumberConverter))]
    public required bool ApprovedForProgressMinimap { get; set; }
    public required double? AscentBetweenFirstLastLrCPsInMeters { get; set; }
    public required double AscentInMeters { get; set; }
    [JsonConverter(typeof(BoolAsNumberConverter))]
    public required bool? BikeRec { get; set; }
    public required uint? BikeType { get; set; }
    [JsonConverter(typeof(BoolAsNumberConverter))]
    public required bool BlockedForClubs { get; set; }
    public required bool BlockedForMeetups { get; set; }
    [JsonConverter(typeof(BoolAsNumberConverter))]
    public required bool BlockedForTimeTrial { get; set; }
    public required int? CheckPointCount { get; set; }
    public required double? DefaultLeadinAscentInMeters { get; set; }
    public required double? DefaultLeadinDistanceInMeters { get; set; }
    public required double? Difficulty { get; set; }
    public required double? DistanceBetweenFirstLastLrCPsInMeters { get; set; }
    public required double DistanceInMeters { get; set; }
    public required int? Duration { get; set; }
    public required bool? EventOnly { get; set; }
    public required string? EventPaddocks { get; set; }
    [JsonConverter(typeof(BoolAsNumberConverter))]
    public required bool ExcludeFromGameDictionary { get; set; }
    public required double? FreeRideLeadinAscentInMeters { get; set; }
    public required double? FreeRideLeadinDistanceInMeters { get; set; }
    public required string? GameDictName { get; set; }
    [JsonConverter(typeof(BoolAsNumberConverter))]
    public required bool HasMatchingLeaderboardSegment { get; set; }
    [JsonConverter(typeof(BoolAsNumberConverter))]
    public required bool HasPortalRoad { get; set; }
    public required uint Id { get; set; }
    public required double LeadinAscentInMeters { get; set; }
    public required double LeadinDistanceInMeters { get; set; }
    public required int? LevelLocked { get; set; }
    public required ZwiftSauceRouteManifest[] Manifest { get; set; }
    public required double? MeetupLeadinAscentInMeters { get; set; }
    public required double? MeetupLeadinDistanceInMeters { get; set; }
    public required string Name { get; set; }
    public required int? NormalDecisionCount { get; set; }
    public required ZwiftSauceRoutePrivateSpawnArea? PrivateSpawnArea { get; set; }
    public required DateTime? PublishedOn { get; set; }
    [JsonConverter(typeof(BoolAsNumberConverter))]
    public required bool RouteEndsUnderTimingArch { get; set; }
    public required string? RouteName { get; set; }
    [JsonConverter(typeof(BoolAsNumberConverter))]
    public required bool? RowRec { get; set; }
    [JsonConverter(typeof(BoolAsNumberConverter))]
    public required bool? RunRec { get; set; }
    public required double? ScentBetweenFirstLastLrCPsInMeters { get; set; }
    public required string? ShowProgressInHUD { get; set; }
    public required ZwiftSauceRouteSpawnArea? SpawnArea { get; set; }
    public required double? SpeedScaledAscentInMeters { get; set; }
    public required double? SpeedScaledDistanceInMeters { get; set; }
    public required double? SpeedScaledFreeRideLeadinAscentInMeters { get; set; }
    public required double? SpeedScaledFreeRideLeadinDistanceInMeters { get; set; }
    public required double? SpeedScaledLeadinAscentInMeters { get; set; }
    public required double? SpeedScaledLeadinDistanceInMeters { get; set; }
    public required double? SpeedScaledMeetupLeadinAscentInMeters { get; set; }
    public required double? SpeedScaledMeetupLeadinDistanceInMeters { get; set; }
    public required int? SportType { get; set; }
    public required bool? SupportedLaps { get; set; }
    [JsonConverter(typeof(BoolAsNumberConverter))]
    public required bool SupportsTimeTrialMode { get; set; }
    public required bool UseAlternateEventRamp { get; set; }
    [JsonConverter(typeof(BoolAsNumberConverter))]
    public required bool? WorkoutRec { get; set; }
    public required uint WorldId { get; set; }
    public required uint? Xp { get; set; }
    [JsonConverter(typeof(BoolAsNumberConverter))]
    public required bool ZwiftEventOnly { get; set; }
    public required int CourseId { get; set; }
    public required ZwiftSauceRouteArch[] Arches { get; set; }
    public required ZwiftSauceRouteSegment[] Segments { get; set; }
}

public class ZwiftSauceRouteManifest
{
    public required int[]? Checkpoints { get; set; }
    public required double End { get; set; }
    public required bool? Leadin { get; set; }
    public required bool Reverse { get; set; }
    public required int RoadId { get; set; }
    public required double Start { get; set; }
}

public sealed record ZwiftSauceRoutePrivateSpawnArea
{
    public required double Endtime { get; set; }
    [JsonConverter(typeof(BoolAsNumberConverter))]
    public required bool Forward { get; set; }
    public required uint Road { get; set; }
    public required double Starttime { get; set; }
}

public sealed record ZwiftSauceRouteSpawnArea
{
    public required double Endtime { get; set; }
    [JsonConverter(typeof(BoolAsNumberConverter))]
    public required bool Forward { get; set; }
    public required uint Road { get; set; }
    public required double Starttime { get; set; }
}

public sealed record ZwiftSauceRouteArch
{
    public required bool GivesPowerUp { get; set; }
    public required bool? LeadinOnly { get; set; }
    public required double Offset { get; set; }
    public required string SegmentId { get; set; }
}

public sealed record ZwiftSauceRouteSegment
{
    public required double Distance { get; set; }
    public required string Id { get; set; }
    public required bool? LeadinOnly { get; set; }
    public required double Offset { get; set; }
}
