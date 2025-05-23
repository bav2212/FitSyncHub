using FitSyncHub.GarminConnect.Auth.Models;

namespace FitSyncHub.GarminConnect.Auth.Abstractions;

public interface IGarminAuthProvider
{
    Task<GarminAuthenticationResult?> GetAuthResult(CancellationToken cancellationToken);
}
