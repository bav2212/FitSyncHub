namespace FitSyncHub.Youtube.Options;

public sealed record YoutubeOptions
{
    public required string ApiKey { get; init; }
    public required string ChannelId { get; init; }
}
