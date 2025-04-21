using FitSyncHub.Common.Applications.IntervalsIcu.Models;
using FitSyncHub.GarminConnect.Models.Responses.Workout;

namespace FitSyncHub.GarminConnect.Converters;

public static class GarminConnectToIntervalsIcuWorkoutConverter
{
    public static List<IntervalsIcuWorkoutGroup> ConvertRideToIntervalsIcuStructure(
        WorkoutResponse workout,
        int ftp)
    {
        List<IntervalsIcuWorkoutGroup> result = [];

        foreach (var step in workout.WorkoutSegments[0].WorkoutSteps)
        {
            if (step is WorkoutExecutableStepResponse workoutStep)
            {
                var item = GetIntervalsIcuWorkoutLine(workoutStep, ftp);

                var group = new IntervalsIcuWorkoutGroup
                {
                    BlockInfo = CreateBlockInfo(workoutStep),
                    Items = [item]
                };
                result.Add(group);
            }

            if (step is WorkoutRepeatGroupResponse repeatStep)
            {
                var flattenedSteps = GarminConnectWorkoutHelper.GetFlattenWorkoutSteps(repeatStep)
                    .Select(x => GetIntervalsIcuWorkoutLine(x, ftp))
                    .ToList();

                var group = new IntervalsIcuWorkoutGroup
                {
                    BlockInfo = IntervalsIcuWorkoutGroupBlockInfo.CreateInterval(repeatStep.NumberOfIterations),
                    Items = flattenedSteps
                };
                result.Add(group);
            }
        }

        return result;
    }


    private static IntervalsIcuWorkoutGroupBlockInfo CreateBlockInfo(
        WorkoutExecutableStepResponse workoutStep)
    {
        return GetWorkoutGroupType(workoutStep) switch
        {
            IntervalsIcuWorkoutGroupType.Warmup => IntervalsIcuWorkoutGroupBlockInfo.CreateWarmup(),
            IntervalsIcuWorkoutGroupType.Cooldown => IntervalsIcuWorkoutGroupBlockInfo.CreateCooldown(),
            IntervalsIcuWorkoutGroupType.Interval => IntervalsIcuWorkoutGroupBlockInfo.CreateDefaultInterval(),
            _ => throw new NotImplementedException(),
        };
    }

    private static IntervalsIcuWorkoutLine GetIntervalsIcuWorkoutLine(
        WorkoutExecutableStepResponse workoutStep,
        int ftp)
    {
        var workoutGroupType = GetWorkoutGroupType(workoutStep);

        var (from, to) = (workoutStep.TargetValueOne, workoutStep.TargetValueTwo) switch
        {
            ({ } fromValue, { } toValue) => (
               GarminConnectWorkoutHelper.GetRoundedPercent(fromValue, ftp),
               GarminConnectWorkoutHelper.GetRoundedPercent(toValue, ftp)
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

        return new IntervalsIcuWorkoutLine()
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

    private static IntervalsIcuWorkoutGroupType GetWorkoutGroupType(
       WorkoutExecutableStepResponse workoutStep)
    {
        return workoutStep.StepType.StepTypeKey switch
        {
            "warmup" => IntervalsIcuWorkoutGroupType.Warmup,
            "cooldown" => IntervalsIcuWorkoutGroupType.Cooldown,
            "interval" or "recovery" => IntervalsIcuWorkoutGroupType.Interval,
            _ => throw new NotImplementedException(),
        };
    }
}
