namespace FitSyncHub.Zwift.Models.FRR;

public abstract record FlammeRougeRacingResultBaseModel
{
    public required int Position { get; init; }
    public required string Rider { get; init; }
    public required long RiderId { get; init; }
}
