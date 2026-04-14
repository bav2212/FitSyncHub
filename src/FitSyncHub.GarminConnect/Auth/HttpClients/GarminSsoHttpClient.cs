using FitSyncHub.GarminConnect.Auth.Models;
using FitSyncHub.GarminConnect.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.GarminConnect.Auth.HttpClients;

internal sealed class GarminSsoHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GarminSsoHttpClient> _logger;
    private readonly GarminConnectAuthOptions _authOptions;

    public GarminSsoHttpClient(
        HttpClient httpClient,
        IOptions<GarminConnectAuthOptions> authOptions,
        ILogger<GarminSsoHttpClient> logger)
    {
        _authOptions = authOptions.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> Login(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<string> ResumeLogin(string mfaCode,
       GarminNeedsMfaClientState needMfaState,
       CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
