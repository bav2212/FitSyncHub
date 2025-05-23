namespace FitSyncHub.GarminConnect.Auth.Models;

public record GarminLoginResult
{
    public GarminAuthenticationResult? AuthenticationResult { get; set; }
    public bool MfaRequired { get; set; }
}
