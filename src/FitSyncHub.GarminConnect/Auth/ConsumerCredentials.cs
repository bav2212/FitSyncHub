namespace FitSyncHub.GarminConnect.Auth;

public record ConsumerCredentials
{
    public required string ConsumerKey { get; init; }
    public required string ConsumerSecret { get; init; }
}
