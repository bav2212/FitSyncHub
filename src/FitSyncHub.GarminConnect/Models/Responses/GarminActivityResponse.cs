namespace FitSyncHub.GarminConnect.Models.Responses;

public record GarminActivityResponse
{
    public long ActivityId { get; init; }
    public GarminActivityUUIDResponse? ActivityUUID { get; init; }
    public string? ActivityName { get; init; }
    public long UserProfileId { get; init; }
    public bool IsMultiSportParent { get; init; }
    public GarminActivityTypeResponse? ActivityTypeDTO { get; init; }
    public GarminActivityEventTypeResponse? EventTypeDTO { get; init; }
    public GarminActivityAccessControlRuleResponse? AccessControlRuleDTO { get; init; }
    public GarminActivityTimeZoneUnitResponse? TimeZoneUnitDTO { get; init; }
    public GarminActivityMetadataResponse? MetadataDTO { get; init; }
    public GarminActivitySummaryResponse? SummaryDTO { get; init; }
    public List<GarminActivityConnectIQMeasurementResponse>? ConnectIQMeasurements { get; init; }
    public List<GarminActivitySplitSummaryResponse>? SplitSummaries { get; init; }
}

public record GarminActivityUUIDResponse
{
    public string? Uuid { get; init; }
}

public record GarminActivityTypeResponse
{
    public int TypeId { get; init; }
    public string? TypeKey { get; init; }
    public int ParentTypeId { get; init; }
    public bool IsHidden { get; init; }
    public bool Restricted { get; init; }
    public bool Trimmable { get; init; }
}

public record GarminActivityEventTypeResponse
{
    public int TypeId { get; init; }
    public string? TypeKey { get; init; }
    public int SortOrder { get; init; }
}

public record GarminActivityAccessControlRuleResponse
{
    public int TypeId { get; init; }
    public string? TypeKey { get; init; }
}

public record GarminActivityTimeZoneUnitResponse
{
    public int UnitId { get; init; }
    public string? UnitKey { get; init; }
    public double Factor { get; init; }
    public string? TimeZone { get; init; }
}

public record GarminActivityMetadataResponse
{
    public bool IsOriginal { get; init; }
    public long DeviceApplicationInstallationId { get; init; }
    public long? AgentApplicationInstallationId { get; init; }
    public string? AgentString { get; init; }
    public GarminActivityFileFormatResponse? FileFormat { get; init; }
    public long? AssociatedCourseId { get; init; }
    public DateTime LastUpdateDate { get; init; }
    public DateTime UploadedDate { get; init; }
    public string? VideoUrl { get; init; }
    public bool HasPolyline { get; init; }
    public bool HasChartData { get; init; }
    public bool HasHrTimeInZones { get; init; }
    public bool HasPowerTimeInZones { get; init; }
    public GarminActivityUserInfoResponse? UserInfoDto { get; init; }
    public List<long>? ChildIds { get; init; }
    public List<string>? ChildActivityTypes { get; init; }
    public List<GarminActivitySensorResponse>? Sensors { get; init; }
    public List<object>? ActivityImages { get; init; }
    public string? Manufacturer { get; init; }
    public int? DiveNumber { get; init; }
    public int LapCount { get; init; }
    public long AssociatedWorkoutId { get; init; }
    public bool? IsAtpActivity { get; init; }
    public GarminActivityDeviceMetaDataResponse? DeviceMetaDataDTO { get; init; }
    public bool HasIntensityIntervals { get; init; }
    public bool HasSplits { get; init; }
    public object? EBikeMaxAssistModes { get; init; }
    public object? EBikeBatteryUsage { get; init; }
    public object? EBikeBatteryRemaining { get; init; }
    public object? EBikeAssistModeInfoDTOList { get; init; }
    public object? HasRunPowerWindData { get; init; }
    public object? CalendarEventInfo { get; init; }
    public string? GroupRideUUID { get; init; }
    public bool HasHeatMap { get; init; }
    public bool AutoCalcCalories { get; init; }
    public bool Favorite { get; init; }
    public bool Gcj02 { get; init; }
    public bool? RunPowerWindDataEnabled { get; init; }
    public bool PersonalRecord { get; init; }
    public bool ManualActivity { get; init; }
    public bool Trimmed { get; init; }
    public bool ElevationCorrected { get; init; }
}

public record GarminActivityFileFormatResponse
{
    public int FormatId { get; init; }
    public string? FormatKey { get; init; }
}

public record GarminActivityUserInfoResponse
{
    public long UserProfilePk { get; init; }
    public string? Displayname { get; init; }
    public string? Fullname { get; init; }
    public string? ProfileImageUrlLarge { get; init; }
    public string? ProfileImageUrlMedium { get; init; }
    public string? ProfileImageUrlSmall { get; init; }
    public bool UserPro { get; init; }
}

public record GarminActivitySensorResponse
{
    public string? Manufacturer { get; init; }
    public long SerialNumber { get; init; }
    public string? SourceType { get; init; }
    public string? AntplusDeviceType { get; init; }
    public double? SoftwareVersion { get; init; }
    public string? BatteryStatus { get; init; }
    public int? BatteryLevel { get; init; }
    public string? Sku { get; init; }
    public int? FitProductNumber { get; init; }
}

public record GarminActivityDeviceMetaDataResponse
{
    public string? DeviceId { get; init; }
    public int DeviceTypePk { get; init; }
    public long DeviceVersionPk { get; init; }
}

public record GarminActivitySummaryResponse
{
    public DateTime StartTimeLocal { get; init; }
    public DateTime StartTimeGMT { get; init; }
    public double Distance { get; init; }
    public double Duration { get; init; }
    public double MovingDuration { get; init; }
    public double ElapsedDuration { get; init; }
    public double ElevationGain { get; init; }
    public double AverageSpeed { get; init; }
    public double AverageMovingSpeed { get; init; }
    public double Calories { get; init; }
    public double BmrCalories { get; init; }
    public double AverageHR { get; init; }
    public double MaxHR { get; init; }
    public double MinHR { get; init; }
    public double AverageBikeCadence { get; init; }
    public double MaxBikeCadence { get; init; }
    public double AveragePower { get; init; }
    public double MaxPower { get; init; }
    public double MaxPowerTwentyMinutes { get; init; }
    public double MinPower { get; init; }
    public double NormalizedPower { get; init; }
    public double FunctionalThresholdPower { get; init; }
    public double TotalWork { get; init; }
    public double TrainingEffect { get; init; }
    public double AnaerobicTrainingEffect { get; init; }
    public string? AerobicTrainingEffectMessage { get; init; }
    public string? AnaerobicTrainingEffectMessage { get; init; }
    public double TrainingStressScore { get; init; }
    public double IntensityFactor { get; init; }
    public int TotalNumberOfStrokes { get; init; }
    public double AvgVerticalSpeed { get; init; }
    public double WaterEstimated { get; init; }
    public double MinRespirationRate { get; init; }
    public double MaxRespirationRate { get; init; }
    public double AvgRespirationRate { get; init; }
    public string? TrainingEffectLabel { get; init; }
    public double ActivityTrainingLoad { get; init; }
    public double MinActivityLapDuration { get; init; }
    public int? DirectWorkoutFeel { get; init; }
    public int? DirectWorkoutRpe { get; init; }
    public int DirectWorkoutComplianceScore { get; init; }
    public double BeginPotentialStamina { get; init; }
    public double EndPotentialStamina { get; init; }
    public double MinAvailableStamina { get; init; }
    public double AvgElapsedDurationVerticalSpeed { get; init; }
    public int DifferenceBodyBattery { get; init; }
}

public record GarminActivityConnectIQMeasurementResponse
{
    public string? AppID { get; init; }
    public int DeveloperFieldNumber { get; init; }
    public string? Value { get; init; }
}

public record GarminActivitySplitSummaryResponse
{
    public double Distance { get; init; }
    public double Duration { get; init; }
    public double MovingDuration { get; init; }
    public double AverageSpeed { get; init; }
    public double Calories { get; init; }
    public double BmrCalories { get; init; }
    public double AverageHR { get; init; }
    public double MaxHR { get; init; }
    public double AverageBikeCadence { get; init; }
    public double MaxBikeCadence { get; init; }
    public double AveragePower { get; init; }
    public double MaxPower { get; init; }
    public double NormalizedPower { get; init; }
    public int TotalExerciseReps { get; init; }
    public string? SplitType { get; init; }
    public int NoOfSplits { get; init; }
    public double MaxDistance { get; init; }
    public double MaxDistanceWithPrecision { get; init; }
}
