using StravaWebhooksAzureFunctions.Extensions;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activity;
using StravaWebhooksAzureFunctions.Services.Interfaces;
using System.Net.Http.Headers;

namespace StravaWebhooksAzureFunctions.HttpClients;

public class StravaRestHttpClient : IStravaRestHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IStravaOAuthService _stravaOAuthService;

    public StravaRestHttpClient(
        HttpClient httpClient,
        IStravaOAuthService stravaOAuthService)
    {
        _httpClient = httpClient;
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

        return await response
            .HandleJsonResponse<ActivityModelResponse>(Constants.StravaApiJsonOptions, cancellationToken);
    }
}