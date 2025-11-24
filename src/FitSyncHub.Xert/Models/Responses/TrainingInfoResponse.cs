using System.Text.Json.Serialization;

namespace FitSyncHub.Xert.Models.Responses;

public sealed record TrainingInfoResponse
{
    public required bool Success { get; init; }
    public required double Weight { get; init; }
    public required string Status { get; init; }
    public required Signature Signature { get; init; }
    public required TrainingLoad Tl { get; init; }
    public required string Source { get; init; }
    public required TrainingLoad TargetXSS { get; init; }
    [JsonPropertyName("wotd")]
    public required WorkoutOfTheDay WorkoutOfTheDay { get; init; }
}

public sealed record Signature
{
    public required double Ltp { get; init; }
    public required double Ftp { get; init; }
    public required double Hie { get; init; }
    public required double Pp { get; init; }
}

public sealed record TrainingLoad
{
    public required double Low { get; init; }
    public required double High { get; init; }
    public required double Peak { get; init; }
    public required double Total { get; init; }
}

public sealed record WorkoutOfTheDay
{
    public required string Type { get; init; }
    public string? Name { get; init; }
    public string? WorkoutId { get; init; }
    public string? Description { get; init; }
    public double? Difficulty { get; init; }
    public string? Url { get; init; }
}

