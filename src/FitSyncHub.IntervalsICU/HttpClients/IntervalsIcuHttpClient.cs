namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient
{
    private readonly HttpClient _httpClient;

    public IntervalsIcuHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}
