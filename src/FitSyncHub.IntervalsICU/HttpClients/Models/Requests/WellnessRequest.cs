using FitSyncHub.IntervalsICU.HttpClients.Models.Common;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public sealed record WellnessRequest
{
    public required string Id { get; init; }
    public float? Ctl { get; init; }
    public float? Atl { get; init; }
    public float? RampRate { get; init; }
    public float? CtlLoad { get; init; }
    public float? AtlLoad { get; init; }
    public WellnessSportInfoRequest[]? SportInfo { get; init; }
    public DateTime? Updated { get; init; }
    public float? Weight { get; init; }
    public int? RestingHR { get; init; }
    public float? Hrv { get; init; }
    public float? HrvSDNN { get; init; }
    public WellnessMenstrualPhase? MenstrualPhase { get; init; }
    public WellnessMenstrualPhase? MenstrualPhasePredicted { get; init; }
    public int? KcalConsumed { get; init; }
    public int? SleepSecs { get; init; }
    public float? SleepScore { get; init; }
    public int? SleepQuality { get; init; }
    public float? AvgSleepingHR { get; init; }
    public int? Soreness { get; init; }
    public int? Fatigue { get; init; }
    public int? Stress { get; init; }
    public int? Mood { get; init; }
    public int? Motivation { get; init; }
    public int? Injury { get; init; }
    public float? SpO2 { get; init; }
    public int? Systolic { get; init; }
    public int? Diastolic { get; init; }
    public int? Hydration { get; init; }
    public float? HydrationVolume { get; init; }
    public float? Readiness { get; init; }
    public float? BaevskySI { get; init; }
    public float? BloodGlucose { get; init; }
    public float? Lactate { get; init; }
    public float? BodyFat { get; init; }
    public float? Abdomen { get; init; }
    public float? Vo2max { get; init; }
    public string? Comments { get; init; }
    public int? Steps { get; init; }
    public float? Respiration { get; init; }
    public bool? Locked { get; init; }
}

public sealed record WellnessSportInfoRequest
{
    public WellnessSportInfoType? Type { get; init; }
    public float? Eftp { get; init; }
}
