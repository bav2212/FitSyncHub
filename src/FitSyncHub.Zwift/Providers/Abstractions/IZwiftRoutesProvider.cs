namespace FitSyncHub.Zwift.Providers.Abstractions;

public interface IZwiftRoutesProvider
{
    Task<List<ZwiftDataWorldRoutePair>> GetRoutesInfo(CancellationToken cancellationToken);
}
