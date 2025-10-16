using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;

namespace FitSyncHub.Zwift.Providers.Abstractions;

public record ZwiftDataWorldRoutePair
{
    public required string WorldName { get; init; }
    public required ZwiftGameInfoRoute Route { get; init; }
}
