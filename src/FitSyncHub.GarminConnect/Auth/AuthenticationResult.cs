namespace FitSyncHub.GarminConnect.Auth;

public record AuthenticationResult
{
    public required OAuth2Token OAuthToken2 { get; init; }
    public required string Cookie { get; init; }
}
