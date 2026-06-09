using FitSyncHub.Zwift.Models;

namespace FitSyncHub.Zwift.Providers.Abstractions;

public interface IZwiftRoutesProvider
{
    Task<List<ZwiftRouteModel>> GetRoutesInfo(CancellationToken cancellationToken);
}
