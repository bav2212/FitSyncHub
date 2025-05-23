using FitSyncHub.Common.Abstractions;
using FitSyncHub.Strava.Options;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Functions;

public class StravaApplicationOptionsProvider : IStravaApplicationOptionsProvider
{
    private readonly StravaOptions _options;

    public StravaApplicationOptionsProvider(IOptions<StravaOptions> options)
    {
        _options = options.Value;
    }

    public string ClientId => _options.Auth.ClientId;
    public string ClientSecret => _options.Auth.ClientSecret;
}
