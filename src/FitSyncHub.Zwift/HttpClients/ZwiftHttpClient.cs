namespace FitSyncHub.Zwift.HttpClients;

public partial class ZwiftHttpClient
{
    private readonly HttpClient _httpClient;

    public ZwiftHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}
