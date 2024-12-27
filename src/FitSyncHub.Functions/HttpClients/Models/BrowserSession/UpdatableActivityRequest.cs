namespace FitSyncHub.Functions.HttpClients.Models.BrowserSession;

public record UpdatableActivityRequest
{
    public required bool Commute { get; init; }
    public required bool Trainer { get; init; }
    public required bool? HideFromHome { get; init; }
    public required string? Description { get; init; }
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required string SportType { get; init; }
    public required string? GearId { get; init; }
}
