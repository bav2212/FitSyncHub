namespace FitSyncHub.Common.Applications.IntervalsIcu.Models;

public record IntervalsIcuWorkoutFtpRange : IIntervalsIcuWorkoutFtp
{
    public required int From { get; init; }
    public required int To { get; init; }
    public required bool IsRampRange { get; init; }
}
