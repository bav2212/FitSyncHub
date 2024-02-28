using StravaWebhooksAzureFunctions.Extensions;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses;
using StravaWebhooksAzureFunctions.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace StravaWebhooksAzureFunctions.HttpClients;

public class StravaRestHttpClient : IStravaRestHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IStravaOAuthService _stravaOAuthService;

    public StravaRestHttpClient(HttpClient httpClient, IStravaOAuthService stravaOAuthService)
    {
        _httpClient = httpClient;
        _stravaOAuthService = stravaOAuthService;
    }

    public async Task<ActivityModelResponse> GetActivity(long activityId, long athleteId)
    {
        var tokenResponse = await _stravaOAuthService.RequestToken(athleteId);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"activities/{activityId}");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        var response = await _httpClient.SendAsync(requestMessage);
        return (await response.HandleJsonResponse<ActivityModelResponse>())!;
    }
}
