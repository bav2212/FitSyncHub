using Microsoft.Extensions.Options;

namespace FitSyncHub.Zwift.Options;

public sealed record ZwiftAuthOptions : IOptions<ZwiftAuthOptions>
{
    public required string Username { get; set; }
    public required string Password { get; set; }

    ZwiftAuthOptions IOptions<ZwiftAuthOptions>.Value => this;
}
