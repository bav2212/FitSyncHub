namespace FitSyncHub.GarminConnect.Auth.Models.Response;

public sealed record GarminDiRefreshTokenResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}
