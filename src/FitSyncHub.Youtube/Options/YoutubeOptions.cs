namespace FitSyncHub.Youtube.Options;

public sealed record YoutubeOptions
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string RefreshToken { get; init; }
}
