using FitSyncHub.IntervalsICU.HttpClients.Models.Common;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public sealed record WellnessResponse
{
    public string? Id { get; init; }
    public float? Ctl { get; init; }
    public float? Atl { get; init; }
    public float? RampRate { get; init; }
    public float? CtlLoad { get; init; }
    public float? AtlLoad { get; init; }
    public required WellnessSportInfoResponse[]? SportInfo { get; init; }
    public DateTime? Updated { get; init; }
    public float? Weight { get; init; }
    public int? RestingHR { get; init; }
    /** rMSSD */
    public float? Hrv { get; init; }
    public float? HrvSDNN { get; init; }
    public WellnessMenstrualPhase? MenstrualPhase { get; init; }
    public WellnessMenstrualPhase? MenstrualPhasePredicted { get; init; }
    public int? KcalConsumed { get; init; }
    public int? SleepSecs { get; init; }
    public float? SleepScore { get; init; }
    /** Poor(4), Avg(3), Good(2), Excellent(1) */
    public int? SleepQuality { get; init; }
    public float? AvgSleepingHR { get; init; }
    /** None(0), Low(1), Avg(2), High(3), Extreme(4) */
    public int? Soreness { get; init; }
    /** None(0), Low(1), Avg(2), High(3), Extreme(4) */
    public int? Fatigue { get; init; }
    /** None(0), Low(1), Avg(2), High(3), Extreme(4) */
    public int? Stress { get; init; }
    /** Poor(4), Avg(3), Good(2), Excellent(1) */
    public int? Mood { get; init; }
    /** Poor(4), Avg(3), Good(2), Excellent(1) */
    public int? Motivation { get; init; }
    /** Injured(4), Poor(3), Niggle(2), Excellent(1) */
    public int? Injury { get; init; }
    /** 0-100% */
    public float? SpO2 { get; init; }
    /** blood pressure mmHg */
    public int? Systolic { get; init; }
    public int? Diastolic { get; init; }
    /** Well Hydrated (1), Hydrated(2), Dehydrated(3), Very Dehydrated(4) */
    public int? Hydration { get; init; }
    /** litres consumed */
    public float? HydrationVolume { get; init; }
    public float? Readiness { get; init; }
    public float? BaevskySI { get; init; }
    /** mmol/L */
    public float? BloodGlucose { get; init; }
    /** mmol/L */
    public float? Lactate { get; init; }
    /** % */
    public float? BodyFat { get; init; }
    /** cm */
    public float? Abdomen { get; init; }
    /** ml/kg/min */
    public float? Vo2max { get; init; }
    public string? Comments { get; init; }
    /** rMSSD */
    public int? Steps { get; init; }
    public float? Respiration { get; init; }
    /** Updates via API and integrations ignored */
    public bool? Locked { get; init; }
    /** Indicates value was updated from athlete settings and is not authoritative */
    public bool? TempWeight { get; init; }
    public bool? TempRestingHR { get; init; }
}

public sealed record WellnessSportInfoResponse
{
    public WellnessSportInfoType? Type { get; init; }
    public float? Eftp { get; init; }
}
