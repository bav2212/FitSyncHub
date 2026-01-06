namespace FitSyncHub.Zwift.Models.FRR;

public sealed record FlammeRougeRacingEGapResultModel
{
    public required int Position { get; init; }
    public required string Rider { get; init; }
    public required long RiderId { get; init; }
    public required string EGap { get; init; }
}
