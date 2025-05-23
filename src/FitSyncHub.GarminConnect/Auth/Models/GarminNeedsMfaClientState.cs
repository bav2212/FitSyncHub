namespace FitSyncHub.GarminConnect.Auth.Models;

public record GarminNeedsMfaClientState
{
    public required string Csrf { get; init; }
}
