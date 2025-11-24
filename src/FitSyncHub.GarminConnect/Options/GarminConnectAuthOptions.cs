namespace FitSyncHub.GarminConnect.Options;

public sealed record GarminConnectAuthOptions
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}
