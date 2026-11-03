using FitSyncHub.Zwift.Models;

namespace FitSyncHub.Zwift.Providers.Abstractions;

public interface IZwiftRoutesProvider
{
    Task<ZwiftParsedRoutesResponse> GetRoutes(CancellationToken cancellationToken);
    Task<List<ZwiftRouteModel>> GetRegularRoutes(CancellationToken cancellationToken);
}

public sealed record ZwiftParsedRoutesResponse
{
    public required List<ZwiftRouteModel> RegularRoutes { get; init; }
    public required List<ZwiftRouteModel> ClimbPortalRoutes { get; init; }
    public required List<ZwiftRouteModel> MochiRoutes { get; init; }

    public List<ZwiftRouteModel> AllRoutes => [.. ClimbPortalRoutes, .. RegularRoutes, .. MochiRoutes];
}
