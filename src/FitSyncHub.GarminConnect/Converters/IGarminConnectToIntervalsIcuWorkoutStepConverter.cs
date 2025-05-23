using FitSyncHub.Common.Workouts;
using FitSyncHub.GarminConnect.Models.Responses.Workout;

namespace FitSyncHub.GarminConnect.Converters;

public interface IGarminConnectToIntervalsIcuWorkoutStepConverter
{
    ExecutableWorkoutStep? Convert(GarminWorkoutExecutableStepResponse workoutStep);
}
