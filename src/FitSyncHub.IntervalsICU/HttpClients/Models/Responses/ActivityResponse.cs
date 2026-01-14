using System.Text.Json.Serialization;
using FitSyncHub.Common.Json;
using FitSyncHub.IntervalsICU.HttpClients.Models.Common;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public sealed record ActivityResponse
{
    public required string Id { get; init; }
    public required DateTime StartDateLocal { get; init; }
    public required string Type { get; init; }
    public required bool? IcuIgnoreTime { get; init; }
    public required int? IcuPmCp { get; init; }
    public required int? IcuPmWPrime { get; init; }
    public required int? IcuPmPMax { get; init; }
    public required int? IcuPmFtp { get; init; }
    public required int? IcuPmFtpSecs { get; init; }
    public required int? IcuPmFtpWatts { get; init; }
    public required bool? IcuIgnorePower { get; init; }
    public required double? IcuRollingCp { get; init; }
    public required double? IcuRollingWPrime { get; init; }
    public required double? IcuRollingPMax { get; init; }
    public required int? IcuRollingFtp { get; init; }
    public required int? IcuRollingFtpDelta { get; init; }
    public required int? IcuTrainingLoad { get; init; }
    public required double? IcuAtl { get; init; }
    public required double? IcuCtl { get; init; }
    public required int? PairedEventId { get; init; }
    public required int? IcuFtp { get; init; }
    public required int? IcuJoules { get; init; }
    public required int? IcuRecordingTime { get; init; }
    public required int ElapsedTime { get; init; }
    public required int? IcuWeightedAvgWatts { get; init; }
    public required int? CarbsUsed { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required DateTime StartDate { get; init; }
    public required double? Distance { get; init; }
    public required double? IcuDistance { get; init; }
    public required int? MovingTime { get; init; }
    public required int? CoastingTime { get; init; }
    public required double? TotalElevationGain { get; init; }
    public required string Timezone { get; init; }
    public required bool? Trainer { get; init; }
    public required bool? Commute { get; init; }
    public required double? MaxSpeed { get; init; }
    public required double? AverageSpeed { get; init; }
    public required bool? DeviceWatts { get; init; }
    public required bool? HasHeartrate { get; init; }
    public required int? MaxHeartrate { get; init; }
    public required int? AverageHeartrate { get; init; }
    public required double? AverageCadence { get; init; }
    public required int? Calories { get; init; }
    public required double? AverageTemp { get; init; }
    public required int? MinTemp { get; init; }
    public required int? MaxTemp { get; init; }
    public required double? AvgLrBalance { get; init; }
    public required double? Gap { get; init; }
    public required ActivityGapModel? GapModel { get; init; }
    public required bool? UseElevationCorrection { get; init; }
    public required bool? Race { get; init; }
    public required ActivityGear Gear { get; init; }
    public required double? PerceivedExertion { get; init; }
    public required string DeviceName { get; init; }
    public required string PowerMeter { get; init; }
    public required string PowerMeterSerial { get; init; }
    public required string PowerMeterBattery { get; init; }
    public required double? CrankLength { get; init; }
    public required string ExternalId { get; init; }
    public required int? FileSportIndex { get; init; }
    public required string FileType { get; init; }
    public required string IcuAthleteId { get; init; }
    public required DateTime Created { get; init; }
    public required DateTime IcuSyncDate { get; init; }
    public required DateTime Analyzed { get; init; }
    public required int? IcuWPrime { get; init; }
    public required double? ThresholdPace { get; init; }
    public required List<int> IcuHrZones { get; init; }
    public required List<double> PaceZones { get; init; }
    public required int? Lthr { get; init; }
    public required int? IcuRestingHr { get; init; }
    public required double? IcuWeight { get; init; }
    public required List<int> IcuPowerZones { get; init; }
    public required int? IcuSweetSpotMin { get; init; }
    public required int? IcuSweetSpotMax { get; init; }
    public required int? IcuPowerSpikeThreshold { get; init; }
    public required double? Trimp { get; init; }
    public required int? IcuWarmupTime { get; init; }
    public required int? IcuCooldownTime { get; init; }
    public required int? IcuChatId { get; init; }
    public required bool? IcuIgnoreHr { get; init; }
    public required bool? IgnoreVelocity { get; init; }
    public required bool? IgnorePace { get; init; }
    public required List<ActivityIgnorePart> IgnoreParts { get; init; }
    public required int? IcuTrainingLoadData { get; init; }
    public required List<string> IntervalSummary { get; init; }
    public required string SkylineChartBytes { get; init; }
    public required List<string> StreamTypes { get; init; }
    public required bool? HasWeather { get; init; }
    public required bool? HasSegments { get; init; }
    public required List<string> PowerFieldNames { get; init; }
    public required string PowerField { get; init; }
    public required List<ActivityZoneTime> IcuZoneTimes { get; init; }
    public required List<int> IcuHrZoneTimes { get; init; }
    public required List<int> PaceZoneTimes { get; init; }
    public required List<int> GapZoneTimes { get; init; }
    public required bool? UseGapZoneTimes { get; init; }
    public required ActivityTizOrder? TizOrder { get; init; }
    public required double? PolarizationIndex { get; init; }
    public required List<ActivityAchievement> IcuAchievements { get; init; }
    public required bool? IcuIntervalsEdited { get; init; }
    public required bool? LockIntervals { get; init; }
    public required int? IcuLapCount { get; init; }
    public required int? IcuJoulesAboveFtp { get; init; }
    public required int? IcuMaxWbalDepletion { get; init; }
    public required ActivityHrr IcuHrr { get; init; }
    public required string IcuSyncError { get; init; }
    public required string IcuColor { get; init; }
    public required double? IcuPowerHrZ2 { get; init; }
    public required int? IcuPowerHrZ2Mins { get; init; }
    public required int? IcuCadenceZ2 { get; init; }
    public required int? IcuRpe { get; init; }
    public required int? Feel { get; init; }
    public required double? KgLifted { get; init; }
    public required double? Decoupling { get; init; }
    public required int? IcuMedianTimeDelta { get; init; }
    public required double? P30sExponent { get; init; }
    public required int? WorkoutShiftSecs { get; init; }
    public required string StravaId { get; init; }
    public required int? Lengths { get; init; }
    public required double? PoolLength { get; init; }
    public required double? Compliance { get; init; }
    public required int? CoachTick { get; init; }
    public required ActivitySource Source { get; init; }
    public required int? OauthClientId { get; init; }
    public required string OauthClientName { get; init; }
    public required double? AverageAltitude { get; init; }
    public required double? MinAltitude { get; init; }
    public required double? MaxAltitude { get; init; }
    public required int? PowerLoad { get; init; }
    public required int? HrLoad { get; init; }
    public required int? PaceLoad { get; init; }
    public required ActivityHrLoadType? HrLoadType { get; init; }
    public required ActivityPaceLoadType? PaceLoadType { get; init; }
    public required List<string> Tags { get; init; }
    public required List<ActivityAttachment> Attachments { get; init; }
    public required List<int> RecordingStops { get; init; }
    public required double? AverageWeatherTemp { get; init; }
    public required double? MinWeatherTemp { get; init; }
    public required double? MaxWeatherTemp { get; init; }
    public required double? AverageFeelsLike { get; init; }
    public required double? MinFeelsLike { get; init; }
    public required double? MaxFeelsLike { get; init; }
    public required double? AverageWindSpeed { get; init; }
    public required double? AverageWindGust { get; init; }
    public required int? PrevailingWindDeg { get; init; }
    public required double? HeadwindPercent { get; init; }
    public required double? TailwindPercent { get; init; }
    public required int? AverageClouds { get; init; }
    public required double? MaxRain { get; init; }
    public required double? MaxSnow { get; init; }
    public required int? CarbsIngested { get; init; }
    public required int? RouteId { get; init; }
    public required string Group { get; init; }
    public required double? Pace { get; init; }
    public required int? AthleteMaxHr { get; init; }
    public required double? IcuIntensity { get; init; }
    public required double? IcuEfficiencyFactor { get; init; }
    public required int? SessionRpe { get; init; }
    public required double? AverageStride { get; init; }
    public required double? IcuPowerHr { get; init; }
    public required int? IcuAverageWatts { get; init; }
    public required double? IcuVariabilityIndex { get; init; }
    public required ActivitySubType? SubType { get; init; }
    #region Custom fields
    [JsonPropertyName("Lactate")]
    public double? Lactate { get; init; }
    #endregion Custom fields

    [JsonIgnore]
    public DateTime EndTimeLocal => StartDateLocal.AddSeconds(ElapsedTime);

    [JsonIgnore]
    public bool IsRide => Type.Contains("Ride");
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
    public required double? Distance { get; init; }
    public required bool? Primary { get; init; }
}

public sealed record ActivityIgnorePart
{
    public required int? StartIndex { get; init; }
    public required int? EndIndex { get; init; }
    public required bool? Power { get; init; }
    public required bool? Pace { get; init; }
    public required bool? Hr { get; init; }
}

public sealed record ActivityZoneTime
{
    public required string Id { get; init; }
    public required int? Secs { get; init; }
}

public sealed record ActivityAchievement
{
    public required string Id { get; init; }
    public required ActivityAchievementType Type { get; init; }
    public required string Message { get; init; }
    public required int? Watts { get; init; }
    public required int? Secs { get; init; }
    public required int? Value { get; init; }
    public required double? Distance { get; init; }
    public required double? Pace { get; init; }
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
    public required int? StartIndex { get; init; }
    public required int? EndIndex { get; init; }
    public required int? Secs { get; init; }
    public required int? Value { get; init; }
}

public sealed record ActivityHrr
{
    public required int? StartIndex { get; init; }
    public required int? EndIndex { get; init; }
    public required int? StartTime { get; init; }
    public required int? EndTime { get; init; }
    public required int? StartBpm { get; init; }
    public required int? EndBpm { get; init; }
    public required int? AverageWatts { get; init; }
    [JsonPropertyName("hrr")]
    public required int? HrrValue { get; init; }
}

public sealed record ActivityAttachment
{
    public required string Id { get; init; }
    public required string Filename { get; init; }
    [JsonPropertyName("mimetype")]
    public required string MimeType { get; init; }
    public required string Url { get; init; }
}
