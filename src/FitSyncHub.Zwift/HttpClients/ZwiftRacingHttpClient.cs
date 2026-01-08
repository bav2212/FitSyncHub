using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftRacing;
using FitSyncHub.Zwift.JsonSerializerContexts;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.Zwift.HttpClients;

public sealed class ZwiftRacingHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ZwiftRacingHttpClient> _logger;

    public ZwiftRacingHttpClient(
        HttpClient httpClient,
        ILogger<ZwiftRacingHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<ZwiftRacingEventResponse>> GetEvent(
        long eventId,
        CancellationToken cancellationToken)
    {
        var url = $"api/events/{eventId}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content,
            ZwiftHttpClientRacingGenerationContext.Default.IReadOnlyCollectionZwiftRacingEventResponse)!;
    }

    public Task<ZwiftRacingRiderResponse?> GetRiderHistory(
        long riderId,
        CancellationToken cancellationToken)
    {
        return GetRiderHistory(riderId, default, cancellationToken);
    }

    public async Task<ZwiftRacingRiderResponse?> GetRiderHistory(
        long riderId,
        int? year,
        CancellationToken cancellationToken)
    {
        var queryParams = new Dictionary<string, StringValues>
        {
            { "year", $"{year ?? DateTime.Now.Year}" }
        };

        var url = QueryHelpers.AddQueryString($"api/riders/{riderId}/history", queryParams);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(url, cancellationToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access when fetching rider history for riderId {RiderId}", riderId);
            return default;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return JsonSerializer.Deserialize(content,
                ZwiftHttpClientRacingGenerationContext.Default.ZwiftRacingRiderResponse);
        }

        var jsonDocument = JsonDocument.Parse(content);
        if (jsonDocument.RootElement.TryGetProperty("error", out var errorJsonValue))
        {
            var errorText = errorJsonValue.ToString();
            if (errorText == "API responded with status 404")
            {
                return default;
            }

            _logger.LogWarning("Error from zwiftracing.app: {Error}", errorText);
        }

        response.EnsureSuccessStatusCode();
        // it will not go here, but just to supress compiler error
        return default;
    }
}
