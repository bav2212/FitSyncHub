using FitSyncHub.Common.Workouts;
using FitSyncHub.GarminConnect.Models.Responses.Workout;

namespace FitSyncHub.GarminConnect.Converters;

internal class GarminConnectToIntervalsIcuRideWorkoutStepConverter
    : IGarminConnectToIntervalsIcuWorkoutStepConverter
{
    private readonly int _ftp;

    public GarminConnectToIntervalsIcuRideWorkoutStepConverter(int ftp)
    {
        _ftp = ftp;
    }

    public ExecutableWorkoutStep? Convert(GarminWorkoutExecutableStepResponse workoutStep)
    {
        var stepType = GarminConnectWorkoutHelper.GetStepType(workoutStep);

        var secondsValue = workoutStep.EndConditionValue
            ?? throw new ArgumentException($"{nameof(workoutStep.EndConditionValue)} must be set");
        var time = TimeSpan.FromSeconds(secondsValue);

        if (stepType is WorkoutStepType.Rest)
        {
            return new RideWorkoutStep
            {
                Type = stepType,
                Time = time,
                IsFreeRide = true,
            };
        }

        var (from, to) = (workoutStep.TargetValueOne, workoutStep.TargetValueTwo) switch
        {
            ({ } fromValue, { } toValue) => (
               GarminConnectWorkoutHelper.GetRoundedPercent(fromValue, _ftp),
               GarminConnectWorkoutHelper.GetRoundedPercent(toValue, _ftp)
            ),
            _ => throw new ArgumentException($"{nameof(workoutStep.TargetValueOne)} and {nameof(workoutStep.TargetValueTwo)} must be set"),
        };

        if (stepType is WorkoutStepType.Cooldown)
        {
            // swap from and to for cooldown
            (from, to) = (to, from);
        }

        return new RideWorkoutStep
        {
            Type = stepType,
            Time = time,
            Ftp = new RideFtpRange
            {
                From = from,
                To = to,
                // disable for now, to check how's better
                //IsRampRange = workoutGroupType is IntervalsIcuWorkoutGroupType.Warmup or IntervalsIcuWorkoutGroupType.Cooldown,
                IsRampRange = false,
            }
        };
    }
}
