namespace FitSyncHub.Common.UnitTests;

public record ActivityInfo
{
    public required int NormalizedPower { get; init; }
    public required int Tss { get; init; }
    public required int Ftp { get; init; }
    public required int RollingFtp { get; init; }
    public required int RecordingTime { get; init; }
    public required int ElapsedTime { get; init; }
    public required int MovingTime { get; init; }
}
