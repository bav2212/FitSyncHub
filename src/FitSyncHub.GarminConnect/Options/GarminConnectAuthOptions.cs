using Microsoft.Extensions.Options;

namespace FitSyncHub.GarminConnect.Options;

public sealed record GarminConnectAuthOptions : IOptions<GarminConnectAuthOptions>
{
    public required string Username { get; set; }
    public required string Password { get; set; }

    GarminConnectAuthOptions IOptions<GarminConnectAuthOptions>.Value => this;
}
