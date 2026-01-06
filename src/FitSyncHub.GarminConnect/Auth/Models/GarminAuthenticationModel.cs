namespace FitSyncHub.GarminConnect.Auth.Models;

public sealed record GarminAuthenticationModel

{
    public required GarminOAuth1Token OAuthToken1 { get; init; }
    public required GarminOAuth2Token OAuthToken2 { get; init; }
}
