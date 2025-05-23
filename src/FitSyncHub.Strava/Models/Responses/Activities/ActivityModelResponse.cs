﻿using System.Text.Json.Serialization;

namespace FitSyncHub.Strava.Models.Responses.Activities;

public record ActivityModelResponse
{
    public int? ResourceState { get; init; }
    public ActivityAthlete? Athlete { get; init; }
    public required string Name { get; init; }
    public float? Distance { get; init; }
    public int? MovingTime { get; init; }
    public int? ElapsedTime { get; init; }
    public float? TotalElevationGain { get; init; }
    public string? Type { get; init; }
    public required SportType SportType { get; init; }
    public long? Id { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? StartDateLocal { get; init; }
    [JsonPropertyName("timezone")]
    public string? TimeZone { get; init; }
    public float? UtcOffset { get; init; }
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
    [JsonPropertyName("start_latlng")]
    public float[]? StartLatitudeLongitude { get; init; }
    [JsonPropertyName("end_latlng")]
    public float[]? EndLatitudeLongitude { get; init; }
    public float? AverageSpeed { get; init; }
    public float? MaxSpeed { get; init; }
    public float? AverageCadence { get; init; }
    public float? AverageTemp { get; init; }
    public float? AverageWatts { get; init; }
    public float? MaxWatts { get; init; }
    public float? WeightedAverageWatts { get; init; }
    public float? Kilojoules { get; init; }
    public bool? DeviceWatts { get; init; }
    public bool? HasHeartrate { get; init; }
    public bool? HeartrateOptOut { get; init; }
    public bool? DisplayHideHeartrateOption { get; init; }
    [JsonPropertyName("elev_high")]
    public float? ElevationHigh { get; init; }
    [JsonPropertyName("elev_low")]
    public float? ElevationLow { get; init; }
    public long? UploadId { get; init; }
    public string? UploadIdStr { get; init; }
    public string? ExternalId { get; init; }
    public bool? FromAcceptedTag { get; init; }
    public int? PrCount { get; init; }
    public int? TotalPhotoCount { get; init; }
    public bool? HasKudoed { get; init; }
    public float? SufferScore { get; init; }
    public string? Description { get; init; }
    public float? Calories { get; init; }
    public string? PerceivedExertion { get; init; }
    public bool? PreferPerceivedExertion { get; init; }
    public object[]? SegmentEfforts { get; init; }
    public ActivitySplitsMetric[]? SplitsMetric { get; init; }
    public ActivitySplitsStandard[]? SplitsStandard { get; init; }
    public ActivityPhotos? Photos { get; init; }
    public required ActivityStatsVisibility[] StatsVisibility { get; init; }
    public bool? HideFromHome { get; init; }
    public string? DeviceName { get; init; }
    public string? EmbedToken { get; init; }
    public string? PrivateNote { get; init; }
    public string[]? AvailableZones { get; init; }
}
