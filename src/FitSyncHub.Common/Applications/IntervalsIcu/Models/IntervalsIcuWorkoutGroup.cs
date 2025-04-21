namespace FitSyncHub.Common.Applications.IntervalsIcu.Models;

public record IntervalsIcuWorkoutGroup
{
    public required IntervalsIcuWorkoutGroupBlockInfo BlockInfo { get; init; }
    public required List<IIntervalsIcuWorkoutLine> Items { get; init; }
}
