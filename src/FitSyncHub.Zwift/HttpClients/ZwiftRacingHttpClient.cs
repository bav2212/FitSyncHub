using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftRacing;
using FitSyncHub.Zwift.JsonSerializerContexts;

namespace FitSyncHub.Zwift.HttpClients;

public class ZwiftRacingHttpClient
{
    private readonly HttpClient _httpClient;

    public ZwiftRacingHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ZwiftRacingRiderResponse> GetRiderHistory(
        // it was awful solution with delegating handler to add cookie(cause scoped service resolves two times), so i just pass cookie here
        string cookie,
        long riderId,
        int? year = default,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/riders/{riderId}/history?year={year ?? DateTime.Now.Year}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Cookie", cookie);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content,
            ZwiftRacingGenerationContext.Default.ZwiftRacingRiderResponse)!;
    }
}
