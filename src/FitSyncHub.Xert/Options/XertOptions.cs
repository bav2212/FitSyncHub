using Microsoft.Extensions.Options;

namespace FitSyncHub.Xert.Options;

public sealed record XertOptions : IOptions<XertOptions>
{
    public required XertAuthOptions Credentials { get; set; }

    public sealed record XertAuthOptions
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    XertOptions IOptions<XertOptions>.Value => this;
}
