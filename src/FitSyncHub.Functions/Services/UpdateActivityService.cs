using System.Text;
using System.Text.Json;
using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Functions.Options;
using FitSyncHub.Strava;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models.Requests;
using FitSyncHub.Strava.Models.Responses.Activities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Functions.Services;

public class UpdateActivityService
{
    private readonly IStravaCookieAuthHttpClient _authService;
    private readonly IStravaCookieHttpClient _stravaCookieHttpClient;
    private readonly IStravaRestHttpClient _stravaRestHttpClient;
    private readonly CorrectElevationService _correctElevationService;
    private readonly StravaOptions _stravaOptions;
    private readonly ILogger<UpdateActivityService> _logger;

    public UpdateActivityService(
        IStravaCookieAuthHttpClient authService,
        IStravaCookieHttpClient stravaCookieHttpClient,
        IStravaRestHttpClient stravaRestHttpClient,
        CorrectElevationService correctElevationService,
        IOptions<StravaOptions> options,
        ILogger<UpdateActivityService> logger)
    {
        _authService = authService;
        _stravaCookieHttpClient = stravaCookieHttpClient;
        _stravaRestHttpClient = stravaRestHttpClient;
        _correctElevationService = correctElevationService;
        _stravaOptions = options.Value;
        _logger = logger;
    }

    public async Task UpdateActivity(
        WebhookEventData webhookEventData,
        CancellationToken cancellationToken)
    {
        var activity = await _stravaRestHttpClient.GetActivity(webhookEventData.ActivityId, cancellationToken);

        // temp
        var activityJson = JsonSerializer.Serialize(activity);
        _logger.LogInformation("Activity: {Activity}", activityJson);

        if (activity.Type == Constants.StravaActivityType.Walk)
        {
            await UpdateActivityVisibilityToOnlyMe(webhookEventData, activity, cancellationToken);
            return;
        }

        if (activity.Type == Constants.StravaActivityType.Run)
        {
            await _correctElevationService.CorrectElevation(webhookEventData.ObjectId, cancellationToken);
            return;
        }

        var isOutdoorRide = activity.Type == Constants.StravaActivityType.Ride
            && activity.DeviceName != Constants.WahooSYSTMDeviceName;
        if (isOutdoorRide)
        {
            await CorrectGearIfNeeded(webhookEventData.ObjectId, activity, cancellationToken);
            await _correctElevationService.CorrectElevation(webhookEventData.ObjectId, cancellationToken);
            return;
        }

        _logger.LogInformation("Skip activity with name: {Name}, id: {Id}, cause it's not walk or ride activity. Type: {Type}",
            activity.Name,
            activity.Id,
            activity.Type);
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
            _logger.LogError("Failed login to Strava");
            return;
        }

        var activityId = activity.Id;

        _logger.LogInformation("Update activity visibility to only me for activity {ActivityId}", activityId);
        var response = await _stravaCookieHttpClient.UpdateActivityVisibilityToOnlyMe(
            activity,
            authResponse.Cookies,
            authResponse.AuthenticityToken,
            PrivateNoteFormatter,
            cancellationToken);

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

    private async Task CorrectGearIfNeeded(
        long activityId,
        ActivityModelResponse activity,
        CancellationToken cancellationToken)
    {
        if (activity.DeviceWatts is null)
        {
            _logger.LogWarning("Be careful. Property {PropertyName} is null for ride Id: {RideId} ", nameof(activity.DeviceWatts), activity.Id);
        }

        if (activity.DeviceWatts == true)
        {
            // do not need to update, cause activity was recorded with powermeter (Bike = Boardman)
            _logger.LogInformation("Do not need to update gear, cause ride {RideId} was recorded with powermeter", activity.Id);
            return;
        }

        if (activity.GearId == Constants.MyCityBikeGearId)
        {
            _logger.LogInformation("Skip cause needed GearId is already settled");
            return;
        }

        _logger.LogInformation("Update gear for ride {RideId}", activity.Id);
        var updateModel = new UpdatableActivityRequest
        {
            Commute = activity.Commute,
            Trainer = activity.Trainer,
            HideFromHome = false,
            Description = activity.Description,
            Name = activity.Name,
            SportType = activity.SportType,
            GearId = Constants.MyCityBikeGearId
        };

        await _stravaRestHttpClient.UpdateActivity(activityId, updateModel, cancellationToken);
    }
}
