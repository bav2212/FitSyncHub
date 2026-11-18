namespace FitSyncHub.Strava.HttpClients.Models.Responses.Activities;

public sealed record ActivitySegment
{
    public long? Id { get; init; }
    public int? ResourceState { get; init; }
    public string? Name { get; init; }
    public string? ActivityType { get; init; }
    public float? Distance { get; init; }
    public float? AverageGrade { get; init; }
    public float? MaximumGrade { get; init; }
    public float? ElevationHigh { get; init; }
    public float? ElevationLow { get; init; }
    [System.Text.Json.Serialization.JsonPropertyName("start_latlng")]
    public float[]? StartLatitudeLongitude { get; init; }
    [System.Text.Json.Serialization.JsonPropertyName("end_latlng")]
    public float[]? EndLatitudeLongitude { get; init; }
    public System.Text.Json.JsonElement? ElevationProfile { get; init; }
    public System.Text.Json.JsonElement? ElevationProfiles { get; init; }
    public int? ClimbCategory { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? Country { get; init; }
    public bool? Private { get; init; }
    public bool? Hazardous { get; init; }
    public bool? Starred { get; init; }
}
