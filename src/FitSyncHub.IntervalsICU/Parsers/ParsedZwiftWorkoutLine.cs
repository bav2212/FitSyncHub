namespace FitSyncHub.IntervalsICU.Parsers;

public record ParsedZwiftWorkoutGroup
{
    public required string BlockDescription { get; init; }
    public required List<ParsedZwiftWorkoutLine> Items { get; init; }
}

public record ParsedZwiftWorkoutLine
{
    public required TimeSpan Time { get; init; }
    public required int? Rpm { get; init; }
    public required IParsedZwiftWorkoutFtp? Ftp { get; init; }
    public required bool IsMaxEffort { get; init; }
    public required bool IsFreeRide { get; init; }
}
