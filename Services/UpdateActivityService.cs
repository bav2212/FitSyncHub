using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StravaWebhooksAzureFunctions.Data.Entities;
using StravaWebhooksAzureFunctions.Helpers;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.HttpClients.Models.BrowserSession;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activities;
using StravaWebhooksAzureFunctions.Models;
using StravaWebhooksAzureFunctions.Options;

namespace StravaWebhooksAzureFunctions.Services;

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
        ActivityModelResponse activity,
        CancellationToken cancellationToken)
    {
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
            // do not do it for now
            //await CorrectGearIfNeeded(webhookEventData.OwnerId, webhookEventData.ObjectId, activity, cancellationToken);
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
        long athleteId,
        long activityId,
        ActivityModelResponse activity,
        CancellationToken cancellationToken)
    {
        var mapPolyline = activity.Map!.Polyline!;
        var coordinates = Polyline.Decode(mapPolyline);

        var isCityRide = IsCityRide(coordinates, activity);
        if (!isCityRide)
        {
            _logger.LogInformation("Skip cause ride {RideId} is not city ride", activity.Id);
            return;
        }

        if (activity.GearId == Constants.MyCityBikeGearId)
        {
            _logger.LogInformation("Skip cause needed GearId is already settled");
            return;
        }

        var updateModel = new UpdatableActivityRequest()
        {
            Commute = activity.Commute,
            Trainer = activity.Trainer,
            HideFromHome = true,
            Description = activity.Description,
            Name = activity.Name,
            Type = activity.Type!,
            SportType = activity.SportType,
            GearId = Constants.MyCityBikeGearId
        };

        await _stravaRestHttpClient.UpdateActivity(activityId, athleteId, updateModel, cancellationToken);
    }

    private static bool IsCityRide(List<Coordinate> coordinates, ActivityModelResponse activity)
    {
        var boundaries = Constants.MyMapBoundaries;

        var inBoundariesCoordinatesCount = coordinates.Where(c =>
            c.Latitude >= boundaries.MinLatitude &&
            c.Latitude <= boundaries.MaxLatitude &&
            c.Longitude >= boundaries.MinLongitude &&
            c.Longitude <= boundaries.MaxLongitude).Count();

        if (inBoundariesCoordinatesCount == coordinates.Count)
        {
            return true;
        }

        var distanceInKm = activity.Distance / 1000;

        // distance < 15km && mostly in my boundaries
        if (distanceInKm < 15 && (double)inBoundariesCoordinatesCount / coordinates.Count > 0.6)
        {
            return true;
        }

        return false;
    }
}
