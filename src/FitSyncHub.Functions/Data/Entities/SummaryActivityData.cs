using FitSyncHub.Functions.Data.Entities.Abstractions;
using FitSyncHub.Strava.Models.Responses.Activities;

namespace FitSyncHub.Functions.Data.Entities;

public sealed class SummaryActivityData : DataModel
{
    public int? ResourceState { get; init; }
    public ActivityAthlete? Athlete { get; init; }
    public required string Name { get; init; }
    public float? Distance { get; init; }
    public int? MovingTime { get; init; }
    public int? ElapsedTime { get; init; }
    public float? TotalElevationGain { get; init; }
    public string? Type { get; init; }
    public required string SportType { get; init; }
    public string? ExternalId { get; init; }
    public long? UploadId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? StartDateLocal { get; init; }
    public string? TimeZone { get; init; }
    public float? UtcOffset { get; init; }
    public float[]? StartLatitudeLongitude { get; init; }
    public float[]? EndLatitudeLongitude { get; init; }
    public string? LocationCity { get; init; }
    public string? LocationState { get; init; }
    public string? LocationCountry { get; init; }
    public int? AchievementCount { get; init; }
    public int? KudosCount { get; init; }
    public int? CommentCount { get; init; }
    public int? AthleteCount { get; init; }
    public int? PhotoCount { get; init; }
    public ActivityMap? Map { get; init; }
    public bool Trainer { get; init; }
    public bool Commute { get; init; }
    public bool? Manual { get; init; }
    public bool? Private { get; init; }
    public string? Visibility { get; init; }
    public bool? Flagged { get; init; }
    public string? GearId { get; init; }
    public float? AverageSpeed { get; init; }
    public float? MaxSpeed { get; init; }
    public float? AverageCadence { get; init; }
    public float? AverageWatts { get; init; }
    public float? WeightedAverageWatts { get; init; }
    public float? Kilojoules { get; init; }
    public bool? DeviceWatts { get; init; }
    public bool? HasHeartrate { get; init; }
    public float? AverageHeartrate { get; init; }
    public float? MaxHeartrate { get; init; }
    public float? MaxWatts { get; init; }
    public int? PrCount { get; init; }
    public int? TotalPhotoCount { get; init; }
    public bool? HasKudoed { get; init; }
    public float? SufferScore { get; init; }
}
