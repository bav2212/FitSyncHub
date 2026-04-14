using FitSyncHub.GarminConnect.Auth.Models;

namespace FitSyncHub.GarminConnect.Auth.Abstractions;

public interface IGarminAuthProvider
{
    Task<GarminDiTokenModel?> GetAuthResult(CancellationToken cancellationToken);
}
