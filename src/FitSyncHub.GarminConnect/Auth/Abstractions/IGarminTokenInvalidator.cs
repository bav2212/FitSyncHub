namespace FitSyncHub.GarminConnect.Auth.Abstractions;

public interface IGarminTokenInvalidator
{
    Task Invalidate(CancellationToken cancellationToken);
}
