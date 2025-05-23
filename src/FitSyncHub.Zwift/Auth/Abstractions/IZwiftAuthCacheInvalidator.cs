namespace FitSyncHub.Zwift.Auth.Abstractions;
public interface IZwiftAuthCacheInvalidator
{
    Task Invalidate(CancellationToken cancellationToken);
}
