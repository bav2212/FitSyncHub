using FitSyncHub.GarminConnect.Models.Auth;

namespace FitSyncHub.GarminConnect;

public interface IGarminAuthenticationService
{
    Task<OAuth2Token> RefreshGarminAuthenticationAsync(CancellationToken cancellationToken);
}
