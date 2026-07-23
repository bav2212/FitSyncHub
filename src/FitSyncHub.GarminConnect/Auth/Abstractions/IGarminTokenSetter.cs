using FitSyncHub.GarminConnect.Auth.Models;

namespace FitSyncHub.GarminConnect.Auth.Abstractions;

public interface IGarminTokenSetter
{
    Task SetTokenModel(GarminDiTokenModel tokenModel, CancellationToken cancellationToken);
}
