using System.Text.Json;
using FitSyncHub.Weather.HttpClients.Models.Requests;
using FitSyncHub.Weather.HttpClients.Models.Responses;
using FitSyncHub.Weather.JsonSerializerContexts;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.Weather.HttpClients;

internal sealed partial class OpenMeteoHttpClient : IOpenMeteoHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenMeteoHttpClient> _logger;

    public OpenMeteoHttpClient(HttpClient httpClient, ILogger<OpenMeteoHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<OpenMeteoResponse> GetOpenMeteoArchive(
        OpenMeteoRequest request,
        CancellationToken cancellationToken)
    {
        const string BaseUrl = "https://archive-api.open-meteo.com/v1/archive";

        var queryParams = new Dictionary<string, StringValues>()
            {
                { "latitude", request.Coordinate.Latitude.ToString() },
                { "longitude", request.Coordinate.Longitude.ToString() },
                { "start_date", request.StartDate.ToString("yyyy-MM-dd") },
                { "end_date", request.EndDate.ToString("yyyy-MM-dd") },
                { "hourly", "temperature_2m" },
                // do not change it, cause it will break json deserialization, because the timezone is hardcoded in JsonConverter
                { "timezone", "GMT" },
            };

        var url = QueryHelpers.AddQueryString(BaseUrl, queryParams);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content,
            OpenMeteoArchiveGenerationContext.Default.OpenMeteoResponse)!;
    }

    public async Task<OpenMeteoResponse> GetOpenMeteoForecast(
        OpenMeteoRequest request,
        CancellationToken cancellationToken)
    {
        const string BaseUrl = "https://historical-forecast-api.open-meteo.com/v1/forecast";

        var queryParams = new Dictionary<string, StringValues>()
            {
                { "latitude", request.Coordinate.Latitude.ToString() },
                { "longitude", request.Coordinate.Longitude.ToString() },
                { "start_date", request.StartDate.ToString("yyyy-MM-dd") },
                { "end_date", request.EndDate.ToString("yyyy-MM-dd") },
                { "hourly", "temperature_2m" },
                // do not change it, cause it will break json deserialization, because the timezone is hardcoded in JsonConverter
                { "timezone", "GMT" },
            };

        var url = QueryHelpers.AddQueryString(BaseUrl, queryParams);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content,
            OpenMeteoArchiveGenerationContext.Default.OpenMeteoResponse)!;
    }
}

internal interface IOpenMeteoHttpClient
{
    Task<OpenMeteoResponse> GetOpenMeteoArchive(
        OpenMeteoRequest request,
        CancellationToken cancellationToken);

    Task<OpenMeteoResponse> GetOpenMeteoForecast(
        OpenMeteoRequest request,
        CancellationToken cancellationToken);
}
