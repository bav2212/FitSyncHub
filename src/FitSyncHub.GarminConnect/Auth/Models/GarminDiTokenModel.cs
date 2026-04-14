namespace FitSyncHub.GarminConnect.Auth.Models;

public sealed record GarminDiTokenModel
{
    public required string DiToken { get; init; }
    public required string DiRefreshToken { get; init; }
    public required string DiClientId { get; init; }
}
