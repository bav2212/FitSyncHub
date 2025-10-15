namespace FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftOffline;

public record ZwiftDataGameInfoMap
{
    public required string Name { get; init; }
    public required List<ZwiftDataGameInfoRoute> Routes { get; init; }
}
