using FitSyncHub.GarminConnect.Converters;
using FitSyncHub.GarminConnect.HttpClients;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.GarminConnect.Services;

internal class GarminConnectToIntervalsIcuRideWorkoutStepConverterInitializer
    : IGarminConnectToIntervalsIcuWorkoutStepConverterInitializer
{
    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<GarminConnectToIntervalsIcuRideWorkoutStepConverterInitializer> _logger;

    public GarminConnectToIntervalsIcuRideWorkoutStepConverterInitializer(
        GarminConnectHttpClient garminConnectHttpClient,
        IMemoryCache memoryCache,
        ILogger<GarminConnectToIntervalsIcuRideWorkoutStepConverterInitializer> logger)
    {
        _garminConnectHttpClient = garminConnectHttpClient;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<IGarminConnectToIntervalsIcuWorkoutStepConverter> Initialize(CancellationToken cancellationToken)
    {
        var ftp = await _memoryCache.GetOrCreateAsync("garmin-ftp", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            _logger.LogInformation("Fetching cycling FTP from Garmin Connect");
            return await _garminConnectHttpClient.GetCyclingFtp(cancellationToken: cancellationToken);
        });
        return new GarminConnectToIntervalsIcuRideWorkoutStepConverter(ftp);
    }
}
