using FitSyncHub.Zwift.Models;

namespace FitSyncHub.Zwift.Providers.Abstractions;

public record ZwiftDataWorldRoutePair
{
    public required string WorldName { get; init; }
    public required ZwiftRouteModel Route { get; init; }
}
