using System.Text.Json;
using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Strava;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models.Requests;
using FitSyncHub.Strava.Models.Responses.Activities;
using FitSyncHub.Strava.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Functions.Services;

public class StravaUpdateActivityService
{
    private readonly IStravaHttpClient _stravaHttpClient;
    private readonly string? _cityBikeGearId;
    private readonly ILogger<StravaUpdateActivityService> _logger;

    public StravaUpdateActivityService(
        IStravaHttpClient stravaHttpClient,
        IOptions<StravaOptions> options,
        ILogger<StravaUpdateActivityService> logger)
    {
        _stravaHttpClient = stravaHttpClient;
        _cityBikeGearId = options.Value.CityBikeGearId;
        _logger = logger;
    }

    public async Task UpdateActivity(
        WebhookEventData webhookEventData,
        CancellationToken cancellationToken)
    {
        var activityId = webhookEventData.ActivityId;

        var activity = await _stravaHttpClient.GetActivity(activityId, cancellationToken);

        // temp
        var activityJson = JsonSerializer.Serialize(activity);
        _logger.LogInformation("Activity: {Activity}", activityJson);

        if (activity.Type == Constants.StravaActivityType.Walk)
        {
            await HideActivityFromHome(activityId, cancellationToken);
            return;
        }

        // temp hide workouts
        if (activity.Type == Constants.StravaActivityType.Workout)
        {
            await HideActivityFromHome(activityId, cancellationToken);
            return;
        }

        var isOutdoorRide = activity.Type == Constants.StravaActivityType.Ride
            && activity.DeviceName != Constants.WahooSYSTMDeviceName;
        if (isOutdoorRide)
        {
            await CorrectGearIfNeed(activityId, activity, cancellationToken);
            return;
        }

        // temp hide virtual rides
        var isVirtualRide = activity.Type == Constants.StravaActivityType.VirtualRide;
        if (isVirtualRide)
        {
            // do not want to spam home feed with warmups/cooldowns
            if (IsWarmup(activity) || IsCooldown(activity))
            {
                await HideActivityFromHome(activityId, cancellationToken);
                return;
            }

            // stop hiding virtual rides for autumn/winter. Uncomment next line if need to hide again
            //await HideActivityFromHome(activityId, cancellationToken);
            return;
        }

        _logger.LogInformation("Skip updating activity with name: {Name}, id: {Id}. Type: {Type}",
            activity.Name,
            activity.Id,
            activity.Type);
    }

    private static bool IsWarmup(ActivityModelResponse activity)
    {
        var warmupKeywords = new[]
        {
            "warmup", "warm-up", "warm"
        };

        var isWarmupName = activity.Name.Split([' '], StringSplitOptions.TrimEntries)
            .Intersect(warmupKeywords, StringComparer.InvariantCultureIgnoreCase)
            .Any();

        return isWarmupName;
    }

    private static bool IsCooldown(ActivityModelResponse activity)
    {
        var warmupKeywords = new[]
        {
            "cooldown", "cool-down"
        };

        var isCooldownName = activity.Name.Split([' '], StringSplitOptions.TrimEntries)
            .Intersect(warmupKeywords, StringComparer.InvariantCultureIgnoreCase)
            .Any();

        return isCooldownName;
    }

    private async Task HideActivityFromHome(
        long activityId,
        CancellationToken cancellationToken)
    {
        await _stravaHttpClient.UpdateActivity(activityId, new UpdatableActivityRequest
        {
            HideFromHome = true,
        }, cancellationToken);
    }

    private async Task CorrectGearIfNeed(
        long activityId,
        ActivityModelResponse activity,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_cityBikeGearId))
        {
            _logger.LogWarning("City bike gear Id is null. Cannot update activity Id: {ActivityId}", activityId);
            return;
        }

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

        if (activity.GearId == _cityBikeGearId)
        {
            _logger.LogInformation("Skip cause needed GearId is already settled");
            return;
        }

        _logger.LogInformation("Update gear for ride {RideId}", activity.Id);
        var updateModel = new UpdatableActivityRequest
        {
            GearId = _cityBikeGearId
        };

        await _stravaHttpClient.UpdateActivity(activityId, updateModel, cancellationToken);
    }
}
