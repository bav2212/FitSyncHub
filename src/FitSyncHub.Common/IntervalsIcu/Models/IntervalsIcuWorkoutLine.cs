namespace FitSyncHub.Common.IntervalsIcu.Models;

public record IntervalsIcuWorkoutLine
{
    public required TimeSpan Time { get; init; }
    public required int? Rpm { get; init; }
    public required IIntervalsIcuWorkoutFtp? Ftp { get; init; }
    public required bool IsMaxEffort { get; init; }
    public required bool IsFreeRide { get; init; }
}
