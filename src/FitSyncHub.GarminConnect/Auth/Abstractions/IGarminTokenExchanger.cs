using FitSyncHub.GarminConnect.Auth.Models;

namespace FitSyncHub.GarminConnect.Auth.Abstractions;

public interface IGarminTokenExchanger
{
    Task<GarminAuthenticationModel> ExchangeToken(GarminOAuth1Token oAuth1Token, CancellationToken cancellationToken);
}
