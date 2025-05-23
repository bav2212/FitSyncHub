namespace FitSyncHub.GarminConnect.Auth.Models;

public record GarminConsumerCredentials
{
    public required string ConsumerKey { get; init; }
    public required string ConsumerSecret { get; init; }
}
