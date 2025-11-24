namespace FitSyncHub.Common.Workouts;

public interface IRideFtp;

public sealed record RideFtpRange : IRideFtp
{
    public required int From { get; init; }
    public required int To { get; init; }
    public required bool IsRampRange { get; init; }
}

public sealed record RideFtpSingle : IRideFtp
{
    public required int Value { get; init; }
}
