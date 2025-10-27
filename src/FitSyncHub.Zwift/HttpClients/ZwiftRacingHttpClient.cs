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
        long riderId,
        int? year = default,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/riders/{riderId}/history?year={year ?? DateTime.Now.Year}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content,
            ZwiftRacingGenerationContext.Default.ZwiftRacingRiderResponse)!;
    }
}
