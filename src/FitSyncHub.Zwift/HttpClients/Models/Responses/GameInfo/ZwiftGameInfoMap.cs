namespace FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;

public sealed record ZwiftGameInfoMap
{
    public required string Name { get; init; }
    public required List<ZwiftGameInfoRoute> Routes { get; init; }
}
