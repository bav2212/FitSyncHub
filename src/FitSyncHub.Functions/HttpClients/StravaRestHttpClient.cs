using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FitSyncHub.Functions.Extensions;
using FitSyncHub.Functions.HttpClients.Interfaces;
using FitSyncHub.Functions.HttpClients.Models.BrowserSession;
using FitSyncHub.Functions.HttpClients.Models.Responses.Activities;
using FitSyncHub.Functions.HttpClients.Models.Responses.Athletes;
using FitSyncHub.Functions.Services.Interfaces;
using Microsoft.Extensions.Logging;
using StravaWebhooksAzureFunctions;

namespace FitSyncHub.Functions.HttpClients;

public class StravaRestHttpClient : IStravaRestHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StravaRestHttpClient> _logger;
    private readonly IStravaOAuthService _stravaOAuthService;

    public StravaRestHttpClient(
        HttpClient httpClient,
        ILogger<StravaRestHttpClient> logger,
        IStravaOAuthService stravaOAuthService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _stravaOAuthService = stravaOAuthService;
    }

    public async Task<DetailedAthleteResponse> UpdateAthlete(
        long athleteId,
        float weight,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await _stravaOAuthService.RequestToken(athleteId, cancellationToken);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"athlete?weight={weight}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        return await HandleJsonResponse(response, StravaRestApiSerializerContext.Default.DetailedAthleteResponse, cancellationToken);
    }

    public async Task<List<SummaryActivityModelResponse>> GetActivities(
        long athleteId,
        long before,
        long after,
        int page,
        int perPage,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await _stravaOAuthService.RequestToken(athleteId, cancellationToken);

        var queryParams = new Dictionary<string, string>()
        {
            { "before", before.ToString() },
            { "after", after.ToString() },
            { "page",  page.ToString() },
            { "per_page",  perPage.ToString() }
        };

        var uri = $"athlete/activities?{string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        return await HandleJsonResponse(response, StravaRestApiSerializerContext.Default.ListSummaryActivityModelResponse, cancellationToken);
    }

    public async Task<ActivityModelResponse> GetActivity(
        long activityId,
        long athleteId,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await _stravaOAuthService.RequestToken(athleteId, cancellationToken);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"activities/{activityId}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        return await HandleJsonResponse(response, StravaRestApiSerializerContext.Default.ActivityModelResponse, cancellationToken);
    }

    public async Task<List<SummaryGearResponse>> GetBikes(
        long athleteId,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await _stravaOAuthService.RequestToken(athleteId, cancellationToken);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "athlete");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        var detailedAthlete =
            await HandleJsonResponse(response, StravaRestApiSerializerContext.Default.DetailedAthleteResponse, cancellationToken);

        return detailedAthlete.Bikes;
    }

    public async Task<ActivityModelResponse> UpdateActivity(
       long activityId,
       long athleteId,
       UpdatableActivityRequest model,
       CancellationToken cancellationToken)
    {
        var tokenResponse = await _stravaOAuthService.RequestToken(athleteId, cancellationToken);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"activities/{activityId}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);
        requestMessage.Content = JsonContent.Create(model, StravaBrowserSessionSerializerContext.Default.UpdatableActivityRequest);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        return await HandleJsonResponse(response, StravaRestApiSerializerContext.Default.ActivityModelResponse, cancellationToken);
    }


    private async Task<T> HandleJsonResponse<T>(HttpResponseMessage response, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken)
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
    }
}
