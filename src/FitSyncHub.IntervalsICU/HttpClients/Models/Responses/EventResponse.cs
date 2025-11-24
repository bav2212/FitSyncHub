using System.Text.Json.Serialization;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public sealed record EventResponse
{
    public required int Id { get; init; }
    public required DateTime StartDateLocal { get; init; }
    // string in API, maybe cause it's not interals.icu field. Maybe it's comming from Strava/Garmin directly?
    public required string Type { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public bool? Indoor { get; init; }
    public required List<string>? Tags { get; init; }
    public string? PairedActivityId { get; init; }
    public long? IcuTrainingLoad { get; init; }
    public long? MovingTime { get; init; }
    public long? Joules { get; init; }
    [JsonPropertyName("workout_doc")]
    public required EventWorkoutDocument WorkoutDocument { get; init; }
}

