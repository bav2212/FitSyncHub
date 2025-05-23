namespace FitSyncHub.Zwift.Auth.Abstractions;

public interface IZwiftAuthenticator
{
    Task<ZwiftAuthToken> Authenticate(CancellationToken cancellationToken);
}
