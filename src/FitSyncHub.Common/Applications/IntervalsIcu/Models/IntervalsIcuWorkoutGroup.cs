namespace FitSyncHub.Common.Applications.IntervalsIcu.Models;

public record IntervalsIcuWorkoutGroup
{
    public required IntervalsIcuWorkoutGroupBlockInfo BlockInfo { get; init; }
    public required List<IntervalsIcuWorkoutLine> Items { get; init; }
}
