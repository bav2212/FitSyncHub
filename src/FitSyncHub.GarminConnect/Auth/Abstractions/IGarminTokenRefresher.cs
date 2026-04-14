using FitSyncHub.GarminConnect.Auth.Models;

namespace FitSyncHub.GarminConnect.Auth.Abstractions;

public interface IGarminTokenRefresher
{
    Task<GarminDiTokenModel> Refresh(GarminDiTokenModel tokenModel, CancellationToken cancellationToken);
}
