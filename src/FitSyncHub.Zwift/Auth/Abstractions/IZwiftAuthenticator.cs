namespace FitSyncHub.Zwift.Auth.Abstractions;

public interface IZwiftAuthenticator
{
    Task<ZwiftAuthTokenModel> Authenticate(CancellationToken cancellationToken);
}
