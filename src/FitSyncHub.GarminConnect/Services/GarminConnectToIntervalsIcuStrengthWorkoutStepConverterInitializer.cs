using FitSyncHub.GarminConnect.Converters;
using FitSyncHub.GarminConnect.HttpClients;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.GarminConnect.Services;

internal class GarminConnectToIntervalsIcuStrengthWorkoutStepConverterInitializer
    : IGarminConnectToIntervalsIcuWorkoutStepConverterInitializer
{
    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<GarminConnectToIntervalsIcuStrengthWorkoutStepConverterInitializer> _logger;

    public GarminConnectToIntervalsIcuStrengthWorkoutStepConverterInitializer(
        GarminConnectHttpClient garminConnectHttpClient,
        IMemoryCache memoryCache,
        ILogger<GarminConnectToIntervalsIcuStrengthWorkoutStepConverterInitializer> logger)
    {
        _garminConnectHttpClient = garminConnectHttpClient;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<IGarminConnectToIntervalsIcuWorkoutStepConverter> Initialize(CancellationToken cancellationToken)
    {
        var translations = await _memoryCache.GetOrCreateAsync("garmin-exrcise-types", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
            _logger.LogInformation("Fetching exercise types translations from Garmin Connect");
            return await _garminConnectHttpClient.GetExerciseTypesTranslations(cancellationToken);
        });

        return new GarminConnectToIntervalsIcuStrengthWorkoutStepConverter(translations!);
    }
}
