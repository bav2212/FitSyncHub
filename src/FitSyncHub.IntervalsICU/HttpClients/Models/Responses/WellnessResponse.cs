using FitSyncHub.IntervalsICU.HttpClients.Models.Common;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public sealed record WellnessResponse
{
    public required string? Id { get; init; }
    public required float? Ctl { get; init; }
    public required float? Atl { get; init; }
    public required float? RampRate { get; init; }
    public required float? CtlLoad { get; init; }
    public required float? AtlLoad { get; init; }
    public required WellnessSportInfoResponse[]? SportInfo { get; init; }
    public required DateTime? Updated { get; init; }
    public required float? Weight { get; init; }
    public required int? RestingHR { get; init; }
    public required float? Hrv { get; init; }
    public required float? HrvSDNN { get; init; }
    public required WellnessMenstrualPhase? MenstrualPhase { get; init; }
    public required WellnessMenstrualPhase? MenstrualPhasePredicted { get; init; }
    public required int? KcalConsumed { get; init; }
    public required int? SleepSecs { get; init; }
    public required float? SleepScore { get; init; }
    public required int? SleepQuality { get; init; }
    public required float? AvgSleepingHR { get; init; }
    public required int? Soreness { get; init; }
    public required int? Fatigue { get; init; }
    public required int? Stress { get; init; }
    public required int? Mood { get; init; }
    public required int? Motivation { get; init; }
    public required int? Injury { get; init; }
    public required float? SpO2 { get; init; }
    public required int? Systolic { get; init; }
    public required int? Diastolic { get; init; }
    public required int? Hydration { get; init; }
    public required float? HydrationVolume { get; init; }
    public required float? Readiness { get; init; }
    public required float? BaevskySI { get; init; }
    public required float? BloodGlucose { get; init; }
    public required float? Lactate { get; init; }
    public required float? BodyFat { get; init; }
    public required float? Abdomen { get; init; }
    public required float? Vo2max { get; init; }
    public required string? Comments { get; init; }
    public required int? Steps { get; init; }
    public required float? Respiration { get; init; }
    public required bool? Locked { get; init; }
}

public sealed record WellnessSportInfoResponse
{
    public required WellnessSportInfoType? Type { get; init; }
    public required float? Eftp { get; init; }
}
