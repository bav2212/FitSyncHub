using FitSyncHub.GarminConnect.Auth.Models;

namespace FitSyncHub.GarminConnect.Auth.Abstractions;

public interface IGarminTokenExchanger
{
    Task<GarminAuthenticationResult> ExchangeToken(GarminOAuth1Token oAuth1Token, CancellationToken cancellationToken);
}
