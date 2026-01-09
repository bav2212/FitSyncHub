namespace FitSyncHub.Zwift.Models.FRR;

public sealed record FlammeRougeRacingPointsResultModel : FlammeRougeRacingResultBaseModel
{
    public required int TotalPoints { get; init; }
}
