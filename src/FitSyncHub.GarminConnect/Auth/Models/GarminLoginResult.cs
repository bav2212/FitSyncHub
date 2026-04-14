namespace FitSyncHub.GarminConnect.Auth.Models;

public sealed record GarminLoginResult
{
    public GarminDiTokenModel? DiTokenModel { get; set; }
    public bool MfaRequired { get; set; }
}
