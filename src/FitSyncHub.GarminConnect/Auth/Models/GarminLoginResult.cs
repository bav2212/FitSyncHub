namespace FitSyncHub.GarminConnect.Auth.Models;

public sealed record GarminLoginResult
{
    public GarminAuthenticationModel? AuthenticationResult { get; set; }
    public bool MfaRequired { get; set; }
}
