using FitSyncHub.Common.Applications.IntervalsIcu.Models;
using FitSyncHub.GarminConnect.Models.Responses.Workout;

namespace FitSyncHub.GarminConnect.Converters;

public interface IGarminConnectToIntervalsIcuWorkoutStepConverter
{
    IIntervalsIcuWorkoutLine? Convert(WorkoutExecutableStepResponse workoutStep);
}
