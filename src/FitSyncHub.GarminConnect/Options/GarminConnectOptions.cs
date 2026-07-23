using Microsoft.Extensions.Options;

namespace FitSyncHub.GarminConnect.Options;

// for future use, if we need to add options for GarminConnect module
public sealed record GarminConnectOptions : IOptions<GarminConnectOptions>
{
    GarminConnectOptions IOptions<GarminConnectOptions>.Value => this;
}
