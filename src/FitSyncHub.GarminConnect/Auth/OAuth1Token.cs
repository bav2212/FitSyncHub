namespace FitSyncHub.GarminConnect.Auth;

public record OAuth1Token
{
    public required string Token { get; init; }
    public required string TokenSecret { get; init; }
}
