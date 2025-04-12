namespace FitSyncHub.Common.Applications.IntervalsIcu.Models;

public record IntervalsIcuWorkoutFtpSingle : IIntervalsIcuWorkoutFtp
{
    public required int Value { get; init; }
}
