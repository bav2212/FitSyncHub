namespace FitSyncHub.GarminConnect.Auth.Abstractions;

public interface IGarminAuthCacheInvalidator
{
    Task Invalidate(CancellationToken cancellationToken);
}
