using FitSyncHub.GarminConnect.Auth.Models;

namespace FitSyncHub.GarminConnect.Auth.Abstractions;

public interface IGarminAuthProvider
{
    Task<GarminAuthenticationModel?> GetAuthResult(CancellationToken cancellationToken);
}
