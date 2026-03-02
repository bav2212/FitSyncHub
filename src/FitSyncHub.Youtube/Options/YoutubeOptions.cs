using Microsoft.Extensions.Options;

namespace FitSyncHub.Youtube.Options;

public sealed record YoutubeOptions : IOptions<YoutubeOptions>
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string RefreshToken { get; set; }

    YoutubeOptions IOptions<YoutubeOptions>.Value => this;
}
