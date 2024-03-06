using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activity;
using StravaWebhooksAzureFunctions.Options;

namespace StravaWebhooksAzureFunctions.Services;

public class UpdateActivityService
{
    private readonly IStravaCookieAuthHttpClient _authService;
    private readonly IStravaCookieHttpClient _stravaCookieHttpClient;
    private readonly StravaOptions _stravaOptions;
    private readonly ILogger<UpdateActivityService> _logger;

    public UpdateActivityService(
        IStravaCookieAuthHttpClient authService,
        IStravaCookieHttpClient stravaCookieHttpClient,
        IOptions<StravaOptions> options,
        ILogger<UpdateActivityService> logger)
    {
        _authService = authService;
        _stravaCookieHttpClient = stravaCookieHttpClient;
        _stravaOptions = options.Value;
        _logger = logger;
    }

    public async Task UpdateActivityVisibilityToOnlyMe(
        ActivityModelResponse activity,
        long athleteId,
        CancellationToken cancellationToken)
    {
        _ = athleteId;

        var userName = _stravaOptions.Credentials.Username;
        var password = _stravaOptions.Credentials.Password;

        var authResponse = await _authService.Login(userName, password, cancellationToken);
        if (!authResponse.Success)
        {
            _logger.LogError("Failed to login to Strava");
            return;
        }

        var activityId = activity.Id;
        var response = await _stravaCookieHttpClient.UpdateActivityVisibilityToOnlyMe(
            activity,
            authResponse.Cookies,
            authResponse.AuthenticityToken,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Can not update activity {ActivityId}, Username: {Username}", activityId, userName);
        }
        else
        {
            _logger.LogInformation("Activity {ActivityId} updated to only me", activityId);
        }
    }
}
