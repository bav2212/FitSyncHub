using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FitSyncHub.Functions.Extensions;
using FitSyncHub.Functions.HttpClients.Interfaces;
using FitSyncHub.Functions.HttpClients.Models.BrowserSession;
using FitSyncHub.Functions.HttpClients.Models.Responses.Activities;
using FitSyncHub.Functions.HttpClients.Models.Responses.Athletes;
using FitSyncHub.Functions.JsonSerializerContexts;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.HttpClients;

public class StravaRestHttpClient : IStravaRestHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StravaRestHttpClient> _logger;

    public StravaRestHttpClient(
        HttpClient httpClient,
        ILogger<StravaRestHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<DetailedAthleteResponse> UpdateAthlete(
        float weight,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsync($"athlete?weight={weight}", null, cancellationToken);

        return await HandleJsonResponse(response,
            StravaRestApiSerializerContext.Default.DetailedAthleteResponse, cancellationToken);
    }

    public async Task<List<SummaryActivityModelResponse>> GetActivities(
        long before,
        long after,
        int page,
        int perPage,
        CancellationToken cancellationToken)
    {
        var queryParams = new Dictionary<string, string>()
        {
            { "before", before.ToString() },
            { "after", after.ToString() },
            { "page",  page.ToString() },
            { "per_page",  perPage.ToString() }
        };

        var requestUri = $"athlete/activities?{string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
        var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        return await HandleJsonResponse(response,
            StravaRestApiSerializerContext.Default.ListSummaryActivityModelResponse, cancellationToken);
    }

    public async Task<ActivityModelResponse> GetActivity(
        long activityId,
        CancellationToken cancellationToken)
    {
        var requestUri = $"activities/{activityId}?include_all_efforts=false";
        var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        return await HandleJsonResponse(response,
            StravaRestApiSerializerContext.Default.ActivityModelResponse, cancellationToken);
    }

    public async Task<List<SummaryGearResponse>> GetBikes(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("athlete", cancellationToken);

        var detailedAthlete = await HandleJsonResponse(response,
            StravaRestApiSerializerContext.Default.DetailedAthleteResponse, cancellationToken);

        return detailedAthlete.Bikes;
    }

    public async Task<ActivityModelResponse> UpdateActivity(
       long activityId,
       UpdatableActivityRequest model,
       CancellationToken cancellationToken)
    {
        var requestUri = $"activities/{activityId}";
        var content = JsonContent.Create(model, StravaBrowserSessionSerializerContext.Default.UpdatableActivityRequest);
        var response = await _httpClient.PutAsync(requestUri, content, cancellationToken);

        return await HandleJsonResponse(response,
            StravaRestApiSerializerContext.Default.ActivityModelResponse, cancellationToken);
    }

    private async Task<T> HandleJsonResponse<T>(
        HttpResponseMessage response,
        JsonTypeInfo<T> jsonTypeInfo,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.HandleJsonResponse(jsonTypeInfo, cancellationToken);
        }
        catch (JsonException)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogError("Cannot deserialize json to type: {Type}, json: {json}", nameof(T), responseContent);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while handling json response");
            throw;
        }
    }
}
