using FitSyncHub.GarminConnect.Converters;

namespace FitSyncHub.GarminConnect.Services;

public interface IGarminConnectToIntervalsIcuWorkoutStepConverterInitializer
{
    Task<IGarminConnectToIntervalsIcuWorkoutStepConverter> Initialize(CancellationToken cancellationToken);
}
