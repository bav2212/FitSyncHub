using FitSyncHub.GarminConnect.Converters;
using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.GarminConnect.Models.Responses.Workout;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.GarminConnect.Services;
public class GarminConnectToIntervalsIcuWorkoutStepConverterInitializer
{
    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<GarminConnectToIntervalsIcuWorkoutStepConverterInitializer> _logger;

    public GarminConnectToIntervalsIcuWorkoutStepConverterInitializer(
       GarminConnectHttpClient garminConnectHttpClient,
       IMemoryCache memoryCache,
       ILogger<GarminConnectToIntervalsIcuWorkoutStepConverterInitializer> logger)
    {
        _garminConnectHttpClient = garminConnectHttpClient;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<IGarminConnectToIntervalsIcuWorkoutStepConverter> GetConverter(
        WorkoutResponse workoutResponse, CancellationToken cancellationToken)
    {
        var workoutSportType = workoutResponse.SportType.SportTypeKey;

        if (workoutSportType == "cycling")
        {
            var ftp = await _memoryCache.GetOrCreateAsync("garmin-ftp", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                _logger.LogInformation("Fetching cycling FTP from Garmin Connect");
                return await _garminConnectHttpClient.GetCyclingFtp(cancellationToken: cancellationToken);
            });
            return new GarminConnectToIntervalsIcuRideWorkoutStepConverter(ftp);
        }

        if (workoutSportType == "strength_training")
        {
            var translations = await _memoryCache.GetOrCreateAsync("garmin-exrcise-types", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                _logger.LogInformation("Fetching exercise types translations from Garmin Connect");
                return await _garminConnectHttpClient.GetExerciseTypesTranslations(cancellationToken);
            });

            return new GarminConnectToIntervalsIcuStrengthWorkoutStepConverter(translations!);
        }

        throw new NotImplementedException();
    }
}
