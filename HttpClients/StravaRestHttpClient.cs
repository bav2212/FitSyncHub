using Microsoft.Extensions.Logging;
using StravaWebhooksAzureFunctions.Extensions;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.HttpClients.Models.Requests;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activities;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Athletes;
using StravaWebhooksAzureFunctions.Services.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace StravaWebhooksAzureFunctions.HttpClients;

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

    public async Task<ActivityModelResponse> GetActivity(
        long activityId,
        long athleteId,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await _stravaOAuthService.RequestToken(athleteId, cancellationToken);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"activities/{activityId}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        try
        {
            return await response
                 .HandleJsonResponse<ActivityModelResponse>(Constants.StravaApiJsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            var responseContent = response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogError("Cannot deserialize json to type: {Type}, json: {json}", nameof(ActivityModelResponse), responseContent);
            throw;
        }
    }

    public async Task<List<SummaryGearResponse>> GetBikes(
        long athleteId,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await _stravaOAuthService.RequestToken(athleteId, cancellationToken);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "athlete");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        try
        {
            var content = await response
                 .HandleJsonResponse<DetailedAthleteResponse>(Constants.StravaApiJsonOptions, cancellationToken);

            return content.Bikes;
        }
        catch (JsonException)
        {
            var responseContent = response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogError("Cannot deserialize json to type: {Type}, json: {json}", nameof(ActivityModelResponse), responseContent);
            throw;
        }

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
        requestMessage.Content = JsonContent.Create(model, options: Constants.StravaApiJsonOptions);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        try
        {
            return await response
                 .HandleJsonResponse<ActivityModelResponse>(Constants.StravaApiJsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            var responseContent = response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogError("Cannot deserialize json to type: {Type}, json: {json}", nameof(ActivityModelResponse), responseContent);
            throw;
        }
    }
}