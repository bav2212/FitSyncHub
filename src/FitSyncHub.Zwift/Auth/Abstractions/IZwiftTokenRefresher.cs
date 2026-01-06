namespace FitSyncHub.Zwift.Auth.Abstractions;

public interface IZwiftTokenRefresher
{
    Task<ZwiftAuthTokenModel> RefreshToken(ZwiftAuthTokenModel token, CancellationToken cancellationToken);
}
