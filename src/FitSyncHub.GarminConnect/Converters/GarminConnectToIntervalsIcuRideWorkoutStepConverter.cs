using FitSyncHub.Common.Applications.IntervalsIcu.Models;
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

    public IIntervalsIcuWorkoutLine? Convert(WorkoutExecutableStepResponse workoutStep)
    {
        var workoutGroupType = GarminConnectWorkoutHelper.GetWorkoutGroupType(workoutStep);

        var (from, to) = (workoutStep.TargetValueOne, workoutStep.TargetValueTwo) switch
        {
            ({ } fromValue, { } toValue) => (
               GarminConnectWorkoutHelper.GetRoundedPercent(fromValue, _ftp),
               GarminConnectWorkoutHelper.GetRoundedPercent(toValue, _ftp)
            ),
            _ => throw new ArgumentException($"{nameof(workoutStep.TargetValueOne)} and {nameof(workoutStep.TargetValueTwo)} must be set"),
        };

        if (workoutGroupType is IntervalsIcuWorkoutGroupType.Cooldown)
        {
            // swap from and to for cooldown
            (from, to) = (to, from);
        }

        var secondsValue = workoutStep.EndConditionValue
            ?? throw new ArgumentException($"{nameof(workoutStep.EndConditionValue)} must be set");
        var time = TimeSpan.FromSeconds(secondsValue);

        return new IntervalsIcuRideWorkoutLine()
        {
            Time = time,
            IsFreeRide = false,
            IsMaxEffort = false,
            Rpm = null,
            Ftp = new IntervalsIcuWorkoutFtpRange
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
