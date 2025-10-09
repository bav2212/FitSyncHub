using FitSyncHub.IntervalsICU.Options;
using Microsoft.Extensions.Options;

namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly string _athleteId;

    public IntervalsIcuHttpClient(HttpClient httpClient, IOptions<IntervalsIcuOptions> options)
    {
        _httpClient = httpClient;
        _athleteId = options.Value.AthleteId;
    }
}
