namespace FitSyncHub.GarminConnect.Auth.Models;

public sealed record GarminNeedsMfaClientState
{
    public required string Csrf { get; init; }
}
