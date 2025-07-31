using System.Text.Json.Serialization;

namespace FitSyncHub.GarminConnect.Models.Responses;
public record GarminActivitySearchResponse
{
    public required long ActivityId { get; init; }
    public required string ActivityName { get; init; }
    public string? Description { get; init; }
    public required DateTime StartTimeLocal { get; init; }
    public required DateTime StartTimeGMT { get; init; }
    public required ActivityType ActivityType { get; init; }
    public required EventType EventType { get; init; }
    public string? Comments { get; init; }
    public string? ParentId { get; init; }
    public double? Distance { get; init; }
    public required double Duration { get; init; }
    public required double ElapsedDuration { get; init; }
    public required double MovingDuration { get; init; }
    public double? ElevationGain { get; init; }
    public double? ElevationLoss { get; init; }
    public double? AverageSpeed { get; init; }
    public double? MaxSpeed { get; init; }
    public double? StartLatitude { get; init; }
    public double? StartLongitude { get; init; }
    public required bool HasPolyline { get; init; }
    public required long OwnerId { get; init; }
    public required string OwnerDisplayName { get; init; }
    public required string OwnerFullName { get; init; }
    public required string OwnerProfileImageUrlSmall { get; init; }
    public required string OwnerProfileImageUrlMedium { get; init; }
    public required string OwnerProfileImageUrlLarge { get; init; }
    public required double Calories { get; init; }
    // BMR (Basal Metabolic Rate) calories
    public double? BmrCalories { get; init; }
    public required double AverageHR { get; init; }
    public required double MaxHR { get; init; }
    public double? AverageBikingCadenceInRevPerMinute { get; init; }
    public double? MaxBikingCadenceInRevPerMinute { get; init; }
    public required string[] UserRoles { get; init; }
    public required Privacy Privacy { get; init; }
    public required bool UserPro { get; init; }
    public required bool HasVideo { get; init; }
    public string? VideoUrl { get; init; }
    public required long TimeZoneId { get; init; }
    public required long BeginTimestamp { get; init; }
    public required long SportTypeId { get; init; }
    public double? AvgPower { get; init; }
    public double? MaxPower { get; init; }
    public double? AerobicTrainingEffect { get; init; }
    public double? AnaerobicTrainingEffect { get; init; }
    public double? Strokes { get; init; }
    public double? NormPower { get; init; }
    public double? LeftBalance { get; init; }
    public double? RightBalance { get; init; }
    public double? AvgLeftBalance { get; init; }
    public double? Max20MinPower { get; init; }
    public double? AvgVerticalOscillation { get; init; }
    public double? TrainingStressScore { get; init; }
    public double? IntensityFactor { get; init; }
    public double? VO2MaxValue { get; init; }
    public double? LactateThresholdBpm { get; init; }
    public double? LactateThresholdSpeed { get; init; }
    public required long DeviceId { get; init; }
    public double? MinElevation { get; init; }
    public double? MaxElevation { get; init; }
    public required string Manufacturer { get; init; }
    public string? LocationName { get; init; }
    public required long LapCount { get; init; }
    public double? EndLatitude { get; init; }
    public double? EndLongitude { get; init; }
    public double? WaterEstimated { get; init; }
    [JsonPropertyName("maxAvgPower_1")]
    public double? MaxAvgPower1 { get; init; }

    [JsonPropertyName("maxAvgPower_2")]
    public double? MaxAvgPower2 { get; init; }

    [JsonPropertyName("maxAvgPower_5")]
    public double? MaxAvgPower5 { get; init; }

    [JsonPropertyName("maxAvgPower_10")]
    public double? MaxAvgPower10 { get; init; }

    [JsonPropertyName("maxAvgPower_20")]
    public double? MaxAvgPower20 { get; init; }

    [JsonPropertyName("maxAvgPower_30")]
    public double? MaxAvgPower30 { get; init; }

    [JsonPropertyName("maxAvgPower_60")]
    public double? MaxAvgPower60 { get; init; }

    [JsonPropertyName("maxAvgPower_120")]
    public double? MaxAvgPower120 { get; init; }

    [JsonPropertyName("maxAvgPower_300")]
    public double? MaxAvgPower300 { get; init; }

    [JsonPropertyName("maxAvgPower_600")]
    public double? MaxAvgPower600 { get; init; }

    [JsonPropertyName("maxAvgPower_1200")]
    public double? MaxAvgPower1200 { get; init; }

    [JsonPropertyName("maxAvgPower_1800")]
    public double? MaxAvgPower1800 { get; init; }

    [JsonPropertyName("maxAvgPower_3600")]
    public double? MaxAvgPower3600 { get; init; }

    [JsonPropertyName("maxAvgPower_7200")]
    public double? MaxAvgPower7200 { get; init; }

    [JsonPropertyName("maxAvgPower_18000")]
    public double? MaxAvgPower18000 { get; init; }
    public bool? ExcludeFromPowerCurveReports { get; init; }
    public double? MinRespirationRate { get; init; }
    public double? MaxRespirationRate { get; init; }
    public double? AvgRespirationRate { get; init; }
    public string? TrainingEffectLabel { get; init; }
    public double? ActivityTrainingLoad { get; init; }
    public required double MinActivityLapDuration { get; init; }
    public string? AerobicTrainingEffectMessage { get; init; }
    public string? AnaerobicTrainingEffectMessage { get; init; }
    public long? ModerateIntensityMinutes { get; init; }
    public long? VigorousIntensityMinutes { get; init; }
    public required bool Purposeful { get; init; }
    public required bool Favorite { get; init; }
    public required bool Pr { get; init; }
    public required bool AutoCalcCalories { get; init; }
    public required bool AtpActivity { get; init; }
    public required bool ManualActivity { get; init; }
    public required bool ElevationCorrected { get; init; }
    public required bool DecoDive { get; init; }
    public required bool Parent { get; init; }
}

public record ActivityType
{
    public required long TypeId { get; init; }
    public required string TypeKey { get; init; }
    public required long ParentTypeId { get; init; }
    public required bool IsHidden { get; init; }
    public required bool Restricted { get; init; }
    public required bool Trimmable { get; init; }
}

public record EventType
{
    public required long TypeId { get; init; }
    public required string TypeKey { get; init; }
    public required long SortOrder { get; init; }
}

public record Privacy
{
    public required long TypeId { get; init; }
    public required string TypeKey { get; init; }
}
