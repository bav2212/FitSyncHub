using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StravaWebhooksAzureFunctions.Data.Entities;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.HttpClients.Models.Requests;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activities;
using StravaWebhooksAzureFunctions.Options;
using System.Text;

namespace StravaWebhooksAzureFunctions.Services;

public class UpdateActivityService
{
    private readonly IStravaCookieAuthHttpClient _authService;
    private readonly IStravaCookieHttpClient _stravaCookieHttpClient;
    private readonly IStravaRestHttpClient _stravaRestHttpClient;
    private readonly StravaOptions _stravaOptions;
    private readonly ILogger<UpdateActivityService> _logger;

    public UpdateActivityService(
        IStravaCookieAuthHttpClient authService,
        IStravaCookieHttpClient stravaCookieHttpClient,
        IStravaRestHttpClient stravaRestHttpClient,
        IOptions<StravaOptions> options,
        ILogger<UpdateActivityService> logger)
    {
        _authService = authService;
        _stravaCookieHttpClient = stravaCookieHttpClient;
        _stravaRestHttpClient = stravaRestHttpClient;
        _stravaOptions = options.Value;
        _logger = logger;
    }

    public async Task UpdateActivity(
        WebhookEventData webhookEventData,
        ActivityModelResponse activity,
        CancellationToken cancellationToken)
    {
        if (activity.Type == Constants.StravaActivityType.Walk)
        {
            await UpdateActivityVisibilityToOnlyMe(webhookEventData, activity, cancellationToken);
            return;
        }

        if (activity.Type == Constants.StravaActivityType.Ride)
        {
            //await UpdateGearIfNeeded(webhookEventData.OwnerId, webhookEventData.ObjectId, activity, cancellationToken);
            return;
        }

        _logger.LogInformation("Skip activity with name: {Name}, id: {Id}, cause it's not walk or ride activity. Type: {Type}", activity.Name, activity.Id, activity.Type);
    }

    private async Task UpdateActivityVisibilityToOnlyMe(
       WebhookEventData webhookEventData,
       ActivityModelResponse activity,
       CancellationToken cancellationToken)
    {
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
            PrivateNoteFormatter,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Can not update activity {ActivityId}, Username: {Username}", activityId, userName);
        }
        else
        {
            _logger.LogInformation("Activity {ActivityId} updated to only me", activityId);
        }

        string PrivateNoteFormatter(DateTime utcNow)
        {
            var eventTime = TimeOnly
                .FromDateTime(DateTimeOffset.FromUnixTimeSeconds(webhookEventData.EventTime).UtcDateTime);
            var webhookReceivedAt = TimeOnly
                .FromDateTime(webhookEventData.CreatedOn.UtcDateTime);
            var now = TimeOnly.FromDateTime(utcNow);

            var sb = new StringBuilder();
            sb.Append("Activity saved at ");
            sb.Append(eventTime.ToLongTimeString());
            sb.Append(", webhook proceed at ");
            sb.Append(webhookReceivedAt.ToLongTimeString());
            sb.Append(", activity updated at ");
            sb.Append(now.ToLongTimeString());

            return sb.ToString();
        }
    }

    private async Task UpdateGearIfNeeded(
        long athleteId,
        long activityId,
        ActivityModelResponse activity,
        CancellationToken cancellationToken)
    {
        var bikes = _stravaRestHttpClient.GetBikes(athleteId, cancellationToken);

        var updateModel = new UpdatableActivityRequest()
        {
            Commute = activity.Commute,
            Trainer = activity.Trainer,
            HideFromHome = true,
            Description = activity.Description,
            Name = activity.Name,
            Type = activity.Type!,
            SportType = activity.SportType,
            GearId = activity.GearId
        };

        await _stravaRestHttpClient.UpdateActivity(activityId, athleteId, updateModel, cancellationToken);
    }
}
