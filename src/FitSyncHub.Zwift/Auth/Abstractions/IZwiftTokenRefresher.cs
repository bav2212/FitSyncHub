namespace FitSyncHub.Zwift.Auth.Abstractions;

public interface IZwiftTokenRefresher
{
    Task<ZwiftAuthToken> RefreshToken(ZwiftAuthToken token, CancellationToken cancellationToken);
}
