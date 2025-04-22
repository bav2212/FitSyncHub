namespace FitSyncHub.GarminConnect.HttpClients;

public partial class GarminConnectHttpClient
{
    private readonly HttpClient _httpClient;

    public GarminConnectHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}
