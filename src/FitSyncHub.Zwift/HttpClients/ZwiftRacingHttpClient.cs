using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftRacing;
using FitSyncHub.Zwift.JsonSerializerContexts;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

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
        var queryParams = new Dictionary<string, StringValues>
        {
            { "year", $"{year ?? DateTime.Now.Year}" }
        };

        var url = QueryHelpers.AddQueryString($"api/riders/{riderId}/history", queryParams);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content,
            ZwiftRacingGenerationContext.Default.ZwiftRacingRiderResponse)!;
    }
}
