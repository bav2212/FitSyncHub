namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public class ActivityResponse
{
    public required string Id { get; set; }
    public required string StartDateLocal { get; set; }
    public required string Type { get; set; }
    public required bool? IcuIgnoreTime { get; set; }
    public required int? IcuPmCp { get; set; }
    public required int? IcuPmWPrime { get; set; }
    public required int? IcuPmPMax { get; set; }
    public required int? IcuPmFtp { get; set; }
    public required int? IcuPmFtpSecs { get; set; }
    public required int? IcuPmFtpWatts { get; set; }
    public required bool? IcuIgnorePower { get; set; }
    public required double? IcuRollingCp { get; set; }
    public required double? IcuRollingWPrime { get; set; }
    public required double? IcuRollingPMax { get; set; }
    public required int? IcuRollingFtp { get; set; }
    public required int? IcuRollingFtpDelta { get; set; }
    public required int? IcuTrainingLoad { get; set; }
    public required double? IcuAtl { get; set; }
    public required double? IcuCtl { get; set; }
    public required int? PairedEventId { get; set; }
    public required int? IcuFtp { get; set; }
    public required int? IcuJoules { get; set; }
    public required int? IcuRecordingTime { get; set; }
    public required int? ElapsedTime { get; set; }
    public required int? IcuWeightedAvgWatts { get; set; }
    public required int? CarbsUsed { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string StartDate { get; set; }
    public required double? Distance { get; set; }
    public required double? IcuDistance { get; set; }
    public required int? MovingTime { get; set; }
    public required int? CoastingTime { get; set; }
    public required double? TotalElevationGain { get; set; }
    public required string Timezone { get; set; }
    public required bool? Trainer { get; set; }
    public required bool? Commute { get; set; }
    public required double? MaxSpeed { get; set; }
    public required double? AverageSpeed { get; set; }
    public required bool? DeviceWatts { get; set; }
    public required bool? HasHeartrate { get; set; }
    public required int? MaxHeartrate { get; set; }
    public required int? AverageHeartrate { get; set; }
    public required double? AverageCadence { get; set; }
    public required int? Calories { get; set; }
    public required double? AverageTemp { get; set; }
    public required int? MinTemp { get; set; }
    public required int? MaxTemp { get; set; }
    public required double? AvgLrBalance { get; set; }
    public required double? Gap { get; set; }
    public required string GapModel { get; set; } //ENUM: NONE, STRAVA_RUN
    public required bool? UseElevationCorrection { get; set; }
    public required bool? Race { get; set; }
    public required Gear Gear { get; set; }
    public required double? PerceivedExertion { get; set; }
    public required string DeviceName { get; set; }
    public required string PowerMeter { get; set; }
    public required string PowerMeterSerial { get; set; }
    public required string PowerMeterBattery { get; set; }
    public required double? CrankLength { get; set; }
    public required string ExternalId { get; set; }
    public required int? FileSportIndex { get; set; }
    public required string FileType { get; set; }
    public required string IcuAthleteId { get; set; }
    public required DateTime Created { get; set; }
    public required DateTime IcuSyncDate { get; set; }
    public required DateTime Analyzed { get; set; }
    public required int? IcuWPrime { get; set; }
    public required double? ThresholdPace { get; set; }
    public required List<int> IcuHrZones { get; set; }
    public required List<double> PaceZones { get; set; }
    public required int? Lthr { get; set; }
    public required int? IcuRestingHr { get; set; }
    public required double? IcuWeight { get; set; }
    public required List<int> IcuPowerZones { get; set; }
    public required int? IcuSweetSpotMin { get; set; }
    public required int? IcuSweetSpotMax { get; set; }
    public required int? IcuPowerSpikeThreshold { get; set; }
    public required double? Trimp { get; set; }
    public required int? IcuWarmupTime { get; set; }
    public required int? IcuCooldownTime { get; set; }
    public required int? IcuChatId { get; set; }
    public required bool? IcuIgnoreHr { get; set; }
    public required bool? IgnoreVelocity { get; set; }
    public required bool? IgnorePace { get; set; }
    public required List<IgnorePart> IgnoreParts { get; set; }
    public required int? IcuTrainingLoadData { get; set; }
    public required List<string> IntervalSummary { get; set; }
    public required string SkylineChartBytes { get; set; }
    public required List<string> StreamTypes { get; set; }
    public required bool? HasWeather { get; set; }
    public required bool? HasSegments { get; set; }
    public required List<string> PowerFieldNames { get; set; }
    public required string PowerField { get; set; }
    public required List<ZoneTime> IcuZoneTimes { get; set; }
    public required List<int> IcuHrZoneTimes { get; set; }
    public required List<int> PaceZoneTimes { get; set; }
    public required List<int> GapZoneTimes { get; set; }
    public required bool? UseGapZoneTimes { get; set; }
    public required string TizOrder { get; set; } //ENUM: POWER_HR_PACE, POWER_PACE_HR, HR_POWER_PACE, HR_PACE_POWER, PACE_POWER_HR, PACE_HR_POWER
    public required double? PolarizationIndex { get; set; }
    public required List<Achievement> IcuAchievements { get; set; }
    public required bool? IcuIntervalsEdited { get; set; }
    public required bool? LockIntervals { get; set; }
    public required int? IcuLapCount { get; set; }
    public required int? IcuJoulesAboveFtp { get; set; }
    public required int? IcuMaxWbalDepletion { get; set; }
    public required Hrr IcuHrr { get; set; }
    public required string IcuSyncError { get; set; }
    public required string IcuColor { get; set; }
    public required double? IcuPowerHrZ2 { get; set; }
    public required int? IcuPowerHrZ2Mins { get; set; }
    public required int? IcuCadenceZ2 { get; set; }
    public required int? IcuRpe { get; set; }
    public required int? Feel { get; set; }
    public required double? KgLifted { get; set; }
    public required double? Decoupling { get; set; }
    public required int? IcuMedianTimeDelta { get; set; }
    public required double? P30sExponent { get; set; }
    public required int? WorkoutShiftSecs { get; set; }
    public required string StravaId { get; set; }
    public required int? Lengths { get; set; }
    public required double? PoolLength { get; set; }
    public required double? Compliance { get; set; }
    public required int? CoachTick { get; set; }
    public required string Source { get; set; } //ENUM: STRAVA, UPLOAD, MANUAL, GARMIN_CONNECT, OAUTH_CLIENT, DROPBOX, POLAR, SUUNTO, COROS, WAHOO, ZWIFT
    public required int? OauthClientId { get; set; }
    public required string OauthClientName { get; set; }
    public required double? AverageAltitude { get; set; }
    public required double? MinAltitude { get; set; }
    public required double? MaxAltitude { get; set; }
    public required int? PowerLoad { get; set; }
    public required int? HrLoad { get; set; }
    public required int? PaceLoad { get; set; }
    public required string HrLoadType { get; set; } //ENUM: AVG_HR, HR_ZONES, HRSS
    public required string PaceLoadType { get; set; } //ENUM: SWIM, RUN
    public required List<string> Tags { get; set; }
    public required List<Attachment> Attachments { get; set; }
    public required List<int> RecordingStops { get; set; }
    public required double? AverageWeatherTemp { get; set; }
    public required double? MinWeatherTemp { get; set; }
    public required double? MaxWeatherTemp { get; set; }
    public required double? AverageFeelsLike { get; set; }
    public required double? MinFeelsLike { get; set; }
    public required double? MaxFeelsLike { get; set; }
    public required double? AverageWindSpeed { get; set; }
    public required double? AverageWindGust { get; set; }
    public required int? PrevailingWindDeg { get; set; }
    public required double? HeadwindPercent { get; set; }
    public required double? TailwindPercent { get; set; }
    public required int? AverageClouds { get; set; }
    public required double? MaxRain { get; set; }
    public required double? MaxSnow { get; set; }
    public required int? CarbsIngested { get; set; }
    public required int? RouteId { get; set; }
    public required string Group { get; set; }
    public required double? Pace { get; set; }
    public required int? AthleteMaxHr { get; set; }
    public required double? IcuIntensity { get; set; }
    public required double? IcuEfficiencyFactor { get; set; }
    public required int? SessionRpe { get; set; }
    public required double? AverageStride { get; set; }
    public required double? IcuPowerHr { get; set; }
    public required int? IcuAverageWatts { get; set; }
    public required double? IcuVariabilityIndex { get; set; }
}

public class Gear
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required double? Distance { get; set; }
    public required bool? Primary { get; set; }
}

public class IgnorePart
{
    public required int? StartIndex { get; set; }
    public required int? EndIndex { get; set; }
    public required bool? Power { get; set; }
    public required bool? Pace { get; set; }
    public required bool? Hr { get; set; }
}

public class ZoneTime
{
    public required string Id { get; set; }
    public required int? Secs { get; set; }
}

public class Achievement
{
    public required string Id { get; set; }
    public required string Type { get; set; } // Enum: BEST_POWER, FTP_UP, LTHR_UP, BEST_PACE
    public required string Message { get; set; }
    public required int? Watts { get; set; }
    public required int? Secs { get; set; }
    public required int? Value { get; set; }
    public required double? Distance { get; set; }
    public required double? Pace { get; set; }
    public required Point Point { get; set; }
}

public class Point
{
    public required int? StartIndex { get; set; }
    public required int? EndIndex { get; set; }
    public required int? Secs { get; set; }
    public required int? Value { get; set; }
}

public class Hrr
{
    public required int? StartIndex { get; set; }
    public required int? EndIndex { get; set; }
    public required int? StartTime { get; set; }
    public required int? EndTime { get; set; }
    public required int? StartBpm { get; set; }
    public required int? EndBpm { get; set; }
    public required int? AverageWatts { get; set; }
    public required int? HrrValue { get; set; }
}

public class Attachment
{
    public required string Id { get; set; }
    public required string Filename { get; set; }
    public required string Mimetype { get; set; }
    public required string Url { get; set; }
}
