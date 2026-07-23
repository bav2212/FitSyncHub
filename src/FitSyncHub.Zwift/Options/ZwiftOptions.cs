using Microsoft.Extensions.Options;

namespace FitSyncHub.Zwift.Options;

public sealed record ZwiftOptions : IOptions<ZwiftOptions>
{
    public required ZwiftAuthOptions Credentials { get; set; }

    public sealed record ZwiftAuthOptions
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    ZwiftOptions IOptions<ZwiftOptions>.Value => this;
}
