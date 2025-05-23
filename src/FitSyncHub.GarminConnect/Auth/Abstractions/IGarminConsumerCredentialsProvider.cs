using FitSyncHub.GarminConnect.Auth.Models;

namespace FitSyncHub.GarminConnect.Auth.Abstractions;

internal interface IGarminConsumerCredentialsProvider
{
    Task<GarminConsumerCredentials> GetConsumerCredentials(CancellationToken cancellationToken);
}
