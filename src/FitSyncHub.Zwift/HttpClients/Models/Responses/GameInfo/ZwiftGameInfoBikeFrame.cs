namespace FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;

public sealed record ZwiftGameInfoBikeFrame
{
    public required string Name { get; init; }
    public required long Id { get; init; }
}
