using FitSyncHub.IntervalsICU.Options;
using Microsoft.Extensions.Options;

namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient
{
    private readonly HttpClient _httpClient;

    public IntervalsIcuHttpClient(HttpClient httpClient, IOptions<IntervalsIcuOptions> options)
    {
        _httpClient = httpClient;
        AthleteBaseUrl = $"athlete/{options.Value.AthleteId}";
    }

    internal string AthleteBaseUrl { get; private set; }
}
