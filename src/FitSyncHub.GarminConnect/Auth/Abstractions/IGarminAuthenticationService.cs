namespace FitSyncHub.GarminConnect.Auth.Abstractions;

public interface IGarminAuthenticationService
{
    Task<AuthenticationResult> RefreshGarminAuthenticationAsync(CancellationToken cancellationToken);
}
