namespace FitSyncHub.Xert.Models.Responses;

public record TrainingInfoResponse
{
    public required bool Success { get; init; }
    public required double Weight { get; init; }
    public required string Status { get; init; }
    public required Signature Signature { get; init; }
    public required TrainingLoad Tl { get; init; }
    public required string Source { get; init; }
    public required TrainingLoad TargetXSS { get; init; }
    public required WorkoutOfTheDay Wotd { get; init; }
}

public record Signature
{
    public required double Ltp { get; init; }
    public required double Ftp { get; init; }
    public required double Hie { get; init; }
    public required double Pp { get; init; }
}

public record TrainingLoad
{
    public required double Low { get; init; }
    public required double High { get; init; }
    public required double Peak { get; init; }
    public required double Total { get; init; }
}

public record WorkoutOfTheDay
{
    public required string Name { get; init; }
    public required string WorkoutId { get; init; }
    public required string Description { get; init; }
    public required string Type { get; init; }
    public required double Difficulty { get; init; }
    public required string Url { get; init; }
}

