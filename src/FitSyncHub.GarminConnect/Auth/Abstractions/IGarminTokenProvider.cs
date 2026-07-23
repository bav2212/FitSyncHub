using FitSyncHub.GarminConnect.Auth.Models;

namespace FitSyncHub.GarminConnect.Auth.Abstractions;

public interface IGarminTokenProvider
{
    Task<GarminDiTokenModel?> GetTokenModel(CancellationToken cancellationToken);
}
