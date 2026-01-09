namespace FitSyncHub.Zwift.Models.FRR;

public sealed record FlammeRougeRacingEGapResultModel : FlammeRougeRacingResultBaseModel
{
    public required string EGap { get; init; }
}
