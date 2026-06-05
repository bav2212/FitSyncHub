using System.Text.Json.Serialization;
using FitSyncHub.Common.Json;
using FitSyncHub.IntervalsICU.HttpClients.Models.Common;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public sealed record ActivityResponse
{
    public required string Id { get; init; }
    public required DateTime StartDateLocal { get; init; }
    public required string Type { get; init; }
    public bool? IcuIgnoreTime { get; init; }
    public int? IcuPmCp { get; init; }
    public int? IcuPmWPrime { get; init; }
    public int? IcuPmPMax { get; init; }
    public int? IcuPmFtp { get; init; }
    public int? IcuPmFtpSecs { get; init; }
    public int? IcuPmFtpWatts { get; init; }
    public bool? IcuIgnorePower { get; init; }
    public double? IcuRollingCp { get; init; }
    public double? IcuRollingWPrime { get; init; }
    public double? IcuRollingPMax { get; init; }
    public int? IcuRollingFtp { get; init; }
    public int? IcuRollingFtpDelta { get; init; }
    public int? IcuTrainingLoad { get; init; }
    public double? IcuAtl { get; init; }
    public double? IcuCtl { get; init; }
    public double? SsPMax { get; init; }
    public double? SsWPrime { get; init; }
    public double? SsCp { get; init; }
    public int? PairedEventId { get; init; }
    public int? IcuFtp { get; init; }
    public int? IcuJoules { get; init; }
    public int? IcuRecordingTime { get; init; }
    public required int ElapsedTime { get; init; }
    /** Normalized watts */
    public int? IcuWeightedAvgWatts { get; init; }
    public int? CarbsUsed { get; init; }
    /** Total joules of work / icu_recording_time */
    public int? IcuAverageWatts { get; init; }
    public double? IcuVariabilityIndex { get; init; }
    public double? StrainScore { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    /** ISO-8601 UTC timezone e.g. 2022-12-28T05:56:38Z */
    public required DateTime StartDate { get; init; }
    public double? Distance { get; init; }
    /** Use this one for distance. */
    public double? IcuDistance { get; init; }
    public int? MovingTime { get; init; }
    /** Time spend moving at 1km/h or more while producing 10w or less of power */
    public int? CoastingTime { get; init; }
    public double? TotalElevationGain { get; init; }
    public double? TotalElevationLoss { get; init; }
    /** Java timezone IDs e.g. Africa/Johannesburg */
    public required string Timezone { get; init; }
    public bool? Trainer { get; init; }
    public ActivitySubType? SubType { get; init; }
    public double? MaxSpeed { get; init; }
    public double? AverageSpeed { get; init; }
    public bool? DeviceWatts { get; init; }
    public bool? HasHeartrate { get; init; }
    public int? MaxHeartrate { get; init; }
    public int? AverageHeartrate { get; init; }
    public double? AverageCadence { get; init; }
    public int? Calories { get; init; }
    public double? AverageTemp { get; init; }
    public int? MinTemp { get; init; }
    public int? MaxTemp { get; init; }
    public double? AvgLrBalance { get; init; }
    /** Gradient adjusted pace */
    public double? Gap { get; init; }
    public required ActivityGapModel GapModel { get; init; }
    public bool? UseElevationCorrection { get; init; }
    public required ActivityGear Gear { get; init; }
    public List<ActivityLap>? Laps { get; init; }
    public double? PerceivedExertion { get; init; }
    public required string DeviceName { get; init; }
    public required string PowerMeter { get; init; }
    public required string PowerMeterSerial { get; init; }
    public required string PowerMeterBattery { get; init; }
    /** In millimeters */
    public double? CrankLength { get; init; }
    /** ID of the activity on the service it came from */
    public required string ExternalId { get; init; }
    /** For multisport files the index of this activity (0=first, 1=second etc.) */
    public int? FileSportIndex { get; init; }
    /** Type of file: fit, tcx, gpx */
    public required string FileType { get; init; }
    public required string IcuAthleteId { get; init; }
    public required DateTime Created { get; init; }
    public required DateTime IcuSyncDate { get; init; }
    public required DateTime Analyzed { get; init; }
    public int? IcuWPrime { get; init; }
    public double? PMax { get; init; }
    public double? ThresholdPace { get; init; }
    /** Max HR for each zone so last entry is athlete's max HR */
    public required List<int> IcuHrZones { get; init; }
    /** Percentage of threshold pace for the top of each zone */
    public required List<double> PaceZones { get; init; }
    public int? Lthr { get; init; }
    public int? IcuRestingHr { get; init; }
    public double? IcuWeight { get; init; }
    /** Max watts for each zone as % of FTP with first entry Z1 */
    public required List<int> IcuPowerZones { get; init; }
    /** Sweet spot start as % of FTP (inclusive) */
    public int? IcuSweetSpotMin { get; init; }
    /** Sweet spot end as % of FTP (inclusive) */
    public int? IcuSweetSpotMax { get; init; }
    public int? IcuPowerSpikeThreshold { get; init; }
    public double? Trimp { get; init; }
    public int? IcuWarmupTime { get; init; }
    public int? IcuCooldownTime { get; init; }
    public int? IcuChatId { get; init; }
    public bool? IcuIgnoreHr { get; init; }
    public bool? IgnoreVelocity { get; init; }
    public bool? IgnorePace { get; init; }
    public required List<ActivityIgnorePart> IgnoreParts { get; init; }
    public int? IcuTrainingLoadData { get; init; }
    public List<ActivityInterval>? IcuIntervals { get; init; }
    public List<ActivityIntervalGroup>? IcuGroups { get; init; }
    public required List<string> IntervalSummary { get; init; }
    public required string SkylineChartBytes { get; init; }
    /** These are the stored streams. Others e.g. fixed_watts are computed on the fly */
    public required List<string> StreamTypes { get; init; }
    public bool? HasWeather { get; init; }
    public bool? HasSegments { get; init; }
    /** The names of fields from FIT file that could provide power data */
    public required List<string> PowerFieldNames { get; init; }
    /** The name of the field providing power data for this activity */
    public required string PowerField { get; init; }
    /** Seconds in each power zone */
    public required List<ActivityZoneTime> IcuZoneTimes { get; init; }
    /** Seconds in each heart rate zone */
    public required List<int> IcuHrZoneTimes { get; init; }
    /** Seconds in each pace zone */
    public required List<int> PaceZoneTimes { get; init; }
    /** Seconds in each pace zone using gradient adjusted pace */
    public required List<int> GapZoneTimes { get; init; }
    public bool? UseGapZoneTimes { get; init; }
    public required List<ActivityTizOrder> CustomZones { get; init; }
    public ActivityTizOrder? TizOrder { get; init; }
    public double? PolarizationIndex { get; init; }
    public required List<ActivityAchievement> IcuAchievements { get; init; }
    public bool? IcuIntervalsEdited { get; init; }
    public bool? LockIntervals { get; init; }
    public int? IcuLapCount { get; init; }
    public int? IcuJoulesAboveFtp { get; init; }
    public int? IcuMaxWbalDepletion { get; init; }
    /** Total time for the activity with recording gaps longer than 30s removed */
    public required ActivityHrr IcuHrr { get; init; }
    public required string IcuSyncError { get; init; }
    public required string IcuColor { get; init; }
    public double? IcuPowerHrZ2 { get; init; }
    public int? IcuPowerHrZ2Mins { get; init; }
    public int? IcuCadenceZ2 { get; init; }
    public int? IcuRpe { get; init; }
    public int? Feel { get; init; }
    public double? KgLifted { get; init; }
    public double? Decoupling { get; init; }
    /** Median seconds between data ticks */
    public int? IcuMedianTimeDelta { get; init; }
    public double? P30sExponent { get; init; }
    /** How much to adjust the start of the workout relative to the activity */
    public int? WorkoutShiftSecs { get; init; }
    /** For activities not from strava that replaced strava activities */
    public required string StravaId { get; init; }
    /** Number of lengths for pool swims */
    public int? Lengths { get; init; }
    public double? PoolLength { get; init; }
    public double? Compliance { get; init; }
    public int? CoachTick { get; init; }
    public required ActivitySource Source { get; init; }
    public int? OauthClientId { get; init; }
    public required string OauthClientName { get; init; }
    public double? AverageAltitude { get; init; }
    public double? MinAltitude { get; init; }
    public double? MaxAltitude { get; init; }
    /** Training load computed from power data (TSS) */
    public int? PowerLoad { get; init; }
    /** Training load computed from heart rate data */
    public int? HrLoad { get; init; }
    /** Training load computed from pace data */
    public int? PaceLoad { get; init; }
    public ActivityHrLoadType? HrLoadType { get; init; }
    public ActivityPaceLoadType? PaceLoadType { get; init; }
    public required List<string> Tags { get; init; }
    public required List<ActivityAttachment> Attachments { get; init; }
    /** When the user stopped the device in seconds since the start time */
    public required List<int> RecordingStops { get; init; }
    public double? AverageWeatherTemp { get; init; }
    public double? MinWeatherTemp { get; init; }
    public double? MaxWeatherTemp { get; init; }
    public double? AverageFeelsLike { get; init; }
    public double? MinFeelsLike { get; init; }
    public double? MaxFeelsLike { get; init; }
    public double? AverageWindSpeed { get; init; }
    public double? AverageWindGust { get; init; }
    public int? PrevailingWindDeg { get; init; }
    public double? HeadwindPercent { get; init; }
    public double? TailwindPercent { get; init; }
    public int? AverageClouds { get; init; }
    public double? MaxRain { get; init; }
    public double? MaxSnow { get; init; }
    public int? CarbsIngested { get; init; }
    public int? RouteId { get; init; }
    public bool? Deleted { get; init; }
    public bool? IcuTrainingLoadEdited { get; init; }
    /** Activity has been newly created or its file is being re-processed. This allows activity field scripts to
     * transform fields read from FIT files once and not every time the activity is analyzed. */
    public bool? IsNew { get; init; }
    public bool? Commute { get; init; }
    public bool? Race { get; init; }
    public double? IcuEfficiencyFactor { get; init; }
    public double? IcuPowerHr { get; init; }
    public double? IcuIntensity { get; init; }
    /** In m/s. Uses moving time if available otherwise elapsed time. */
    public double? Pace { get; init; }
    public int? SessionRpe { get; init; }
    public double? AthleteMaxHr { get; init; }
    /** Stride length in meters. */
    public double? AverageStride { get; init; }
    #region Custom fields
    [JsonPropertyName("Lactate")]
    public double? Lactate { get; init; }

    [JsonPropertyName("HSIh")]
    public double? HeatLoad { get; init; }
    #endregion Custom fields

    [JsonIgnore]
    public DateTime EndTimeLocal => StartDateLocal.AddSeconds(ElapsedTime);

    [JsonIgnore]
    public bool IsRide => Type.Contains("Ride");
}

public sealed record ActivityInterval
{
    /** First data point for this interval in the activity streams. Use this to index over stream data for an interval.
   * Example: for (let i = interval.start_index; i < interval.end_index; i++) joules += watts[i] */
    public double? StartIndex { get; init; }
    public double? Distance { get; init; }
    public double? MovingTime { get; init; }
    public double? ElapsedTime { get; init; }
    public double? AverageWatts { get; init; }
    public double? AverageWattsAlt { get; init; }
    public double? AverageWattsAltAcc { get; init; }
    public double? MinWatts { get; init; }
    public double? MaxWatts { get; init; }
    public double? AverageWattsKg { get; init; }
    public double? MaxWattsKg { get; init; }
    public double? Intensity { get; init; }
    public double? W5sVariability { get; init; }
    public double? WeightedAverageWatts { get; init; }
    public double? TrainingLoad { get; init; }
    public double? Joules { get; init; }
    public double? JoulesAboveFtp { get; init; }
    public double? Decoupling { get; init; }
    public double? AvgLrBalance { get; init; }
    public double? AverageDfaA1 { get; init; }
    public double? AverageEpoc { get; init; }
    public double? WbalStart { get; init; }
    public double? WbalEnd { get; init; }
    public double? AverageRespiration { get; init; }
    public double? AverageTidalVolume { get; init; }
    public double? AverageTidalVolumeMin { get; init; }
    public double? Zone { get; init; }
    public double? ZoneMinWatts { get; init; }
    public double? ZoneMaxWatts { get; init; }
    public double? AverageSpeed { get; init; }
    public double? MinSpeed { get; init; }
    public double? MaxSpeed { get; init; }
    public double? Gap { get; init; }
    public double? AverageHeartrate { get; init; }
    public double? MinHeartrate { get; init; }
    public double? MaxHeartrate { get; init; }
    public double? AverageCadence { get; init; }
    public double? MinCadence { get; init; }
    public double? MaxCadence { get; init; }
    public double? AverageTorque { get; init; }
    public double? MinTorque { get; init; }
    public double? MaxTorque { get; init; }
    public double? TotalElevationGain { get; init; }
    public double? MinAltitude { get; init; }
    public double? MaxAltitude { get; init; }
    public double? AverageGradient { get; init; }
    public double? AverageSmo2 { get; init; }
    public double? AverageThb { get; init; }
    public double? AverageSmo22 { get; init; }
    public double? AverageThb2 { get; init; }
    public double? AverageTemp { get; init; }
    public double? AverageWeatherTemp { get; init; }
    public double? AverageFeelsLike { get; init; }
    public double? AverageWindSpeed { get; init; }
    public double? AverageWindGust { get; init; }
    public double? PrevailingWindDeg { get; init; }
    public double? AverageYaw { get; init; }
    public double? HeadwindPercent { get; init; }
    public double? TailwindPercent { get; init; }
    public double? StrainScore { get; init; }
    public double? SsPMax { get; init; }
    public double? SsWPrime { get; init; }
    public double? SsCp { get; init; }
    public double? AverageStride { get; init; }
    public double? Id { get; init; }
    public ActivityIntervalType? Type { get; init; }
    /** Last data point for this interval in the activity streams (exclusive). Use this to index over stream data for an interval. */
    public double? EndIndex { get; init; }
    public string? GroupId { get; init; }
    public double[]? SegmentEffortIds { get; init; }
    /** Start time in seconds relative to the start of the activity. Do not use this to loop over activity stream data, use
     * start_index and end_index instead. */
    public double? StartTime { get; init; }
    public double? EndTime { get; init; }
    public string? Label { get; init; }
}

/**
 * Intervals can be work or recovery. How this is assigned depends on the algorithm.
 */
[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<ActivityIntervalType>))]
public enum ActivityIntervalType
{
    Recovery,
    Work,
}

public sealed record ActivityIntervalGroup
{
    /** First data point for this interval in the activity streams. Use this to index over stream data for an interval.
   * Example: for (let i = interval.start_index; i < interval.end_index; i++) joules += watts[i] */
    public double? StartIndex { get; init; }
    public double? Distance { get; init; }
    public double? MovingTime { get; init; }
    public double? ElapsedTime { get; init; }
    public double? AverageWatts { get; init; }
    public double? AverageWattsAlt { get; init; }
    public double? AverageWattsAltAcc { get; init; }
    public double? MinWatts { get; init; }
    public double? MaxWatts { get; init; }
    public double? AverageWattsKg { get; init; }
    public double? MaxWattsKg { get; init; }
    public double? Intensity { get; init; }
    public double? W5sVariability { get; init; }
    public double? WeightedAverageWatts { get; init; }
    public double? TrainingLoad { get; init; }
    public double? Joules { get; init; }
    public double? JoulesAboveFtp { get; init; }
    public double? Decoupling { get; init; }
    public double? AvgLrBalance { get; init; }
    public double? AverageDfaA1 { get; init; }
    public double? AverageEpoc { get; init; }
    public double? WbalStart { get; init; }
    public double? WbalEnd { get; init; }
    public double? AverageRespiration { get; init; }
    public double? AverageTidalVolume { get; init; }
    public double? AverageTidalVolumeMin { get; init; }
    public double? Zone { get; init; }
    public double? ZoneMinWatts { get; init; }
    public double? ZoneMaxWatts { get; init; }
    public double? AverageSpeed { get; init; }
    public double? MinSpeed { get; init; }
    public double? MaxSpeed { get; init; }
    public double? Gap { get; init; }
    public double? AverageHeartrate { get; init; }
    public double? MinHeartrate { get; init; }
    public double? MaxHeartrate { get; init; }
    public double? AverageCadence { get; init; }
    public double? MinCadence { get; init; }
    public double? MaxCadence { get; init; }
    public double? AverageTorque { get; init; }
    public double? MinTorque { get; init; }
    public double? MaxTorque { get; init; }
    public double? TotalElevationGain { get; init; }
    public double? MinAltitude { get; init; }
    public double? MaxAltitude { get; init; }
    public double? AverageGradient { get; init; }
    public double? AverageSmo2 { get; init; }
    public double? AverageThb { get; init; }
    public double? AverageSmo22 { get; init; }
    public double? AverageThb2 { get; init; }
    public double? AverageTemp { get; init; }
    public double? AverageWeatherTemp { get; init; }
    public double? AverageFeelsLike { get; init; }
    public double? AverageWindSpeed { get; init; }
    public double? AverageWindGust { get; init; }
    public double? PrevailingWindDeg { get; init; }
    public double? AverageYaw { get; init; }
    public double? HeadwindPercent { get; init; }
    public double? TailwindPercent { get; init; }
    public double? StrainScore { get; init; }
    public double? SsPMax { get; init; }
    public double? SsWPrime { get; init; }
    public double? SsCp { get; init; }
    public double? AverageStride { get; init; }
    public string? Id { get; init; }
    public double? Count { get; init; }
}

public sealed record ActivityLap
{
    public double? StartIndex { get; init; }
    public double? EndIndex { get; init; }
    public string? Name { get; init; }
    public ActivityIntensity? Intensity { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<ActivityIntensity>))]
public enum ActivityIntensity
{
    Active,
    Rest,
    Warmup,
    Cooldown,
    Recovery,
    Interval,
    Other,
}

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<ActivityGapModel>))]
public enum ActivityGapModel
{
    None,
    StravaRun,
}

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<ActivityTizOrder>))]
public enum ActivityTizOrder
{
    PowerHrPace,
    PowerPaceHr,
    HrPowerPace,
    HrPacePower,
    PacePowerHr,
    PaceHrPower,
}

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<ActivityPaceLoadType>))]
public enum ActivityPaceLoadType
{
    Swim,
    Run,
}

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<ActivityHrLoadType>))]
public enum ActivityHrLoadType
{
    AvgHr,
    HrZones,
    [JsonStringEnumMemberName("HRSS")]
    HeartRateStressScore,
}

public sealed record ActivityGear
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public double? Distance { get; init; }
    public bool? Primary { get; init; }
}

public sealed record ActivityIgnorePart
{
    public int? StartIndex { get; init; }
    public int? EndIndex { get; init; }
    public bool? Power { get; init; }
    public bool? Pace { get; init; }
    public bool? Hr { get; init; }
}

public sealed record ActivityZoneTime
{
    public required string Id { get; init; }
    public int? Secs { get; init; }
}

public sealed record ActivityAchievement
{
    public required string Id { get; init; }
    public required ActivityAchievementType Type { get; init; }
    public required string Message { get; init; }
    public int? Watts { get; init; }
    public int? Secs { get; init; }
    public int? Value { get; init; }
    public double? Distance { get; init; }
    public double? Pace { get; init; }
    public required ActivityPoint Point { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<ActivityAchievementType>))]
public enum ActivityAchievementType
{
    BestPower,
    FtpUp,
    LthrUp,
    BestPace,
}

public sealed record ActivityPoint
{
    public int? StartIndex { get; init; }
    public int? EndIndex { get; init; }
    public int? Secs { get; init; }
    public int? Value { get; init; }
}

public sealed record ActivityHrr
{
    public int? StartIndex { get; init; }
    public int? EndIndex { get; init; }
    public int? StartTime { get; init; }
    public int? EndTime { get; init; }
    public int? StartBpm { get; init; }
    public int? EndBpm { get; init; }
    public int? AverageWatts { get; init; }
    [JsonPropertyName("hrr")]
    public int? HrrValue { get; init; }
}

public sealed record ActivityAttachment
{
    public required string Id { get; init; }
    public required string Filename { get; init; }
    [JsonPropertyName("mimetype")]
    public required string MimeType { get; init; }
    public required string Url { get; init; }
}
