using Microsoft.Extensions.Logging;
using StravaWebhooksAzureFunctions.Extensions;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activity;
using StravaWebhooksAzureFunctions.Services.Interfaces;
using System.Net.Http.Headers;
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
}