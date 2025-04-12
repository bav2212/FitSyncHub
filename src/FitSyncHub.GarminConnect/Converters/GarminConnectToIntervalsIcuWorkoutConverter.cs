using FitSyncHub.Common.Applications.IntervalsIcu.Models;
using FitSyncHub.GarminConnect.Models.Responses.Workout;

namespace FitSyncHub.GarminConnect.Converters;

public static class GarminConnectToIntervalsIcuWorkoutConverter
{
    public static List<IntervalsIcuWorkoutGroup> ConvertGarminWorkoutToIntervalsIcuWorkoutGroups(
        GarminConnectWorkoutResponse workout, int garminConnectFtp)
    {
        List<IntervalsIcuWorkoutGroup> result = [];

        foreach (var step in workout.WorkoutSegments[0].WorkoutSteps)
        {
            if (step is GarminConnectExecutableStepResponse workoutStep)
            {
                var item = GetIntervalsIcuWorkoutLine(workoutStep, garminConnectFtp);

                var group = new IntervalsIcuWorkoutGroup
                {
                    BlockInfo = CreateBlockInfo(workoutStep),
                    Items = [item]
                };
                result.Add(group);
            }

            if (step is GarminConnectRepeatGroupResponse repeatStep)
            {
                var group = new IntervalsIcuWorkoutGroup
                {
                    BlockInfo = IntervalsIcuWorkoutGroupBlockInfo.CreateInterval(repeatStep.NumberOfIterations),
                    Items = [.. repeatStep.WorkoutSteps
                        .OfType<GarminConnectExecutableStepResponse>()
                        .Select(x => GetIntervalsIcuWorkoutLine(x, garminConnectFtp))]
                };

                result.Add(group);
            }
        }

        return result;
    }

    private static IntervalsIcuWorkoutGroupBlockInfo CreateBlockInfo(
        GarminConnectExecutableStepResponse workoutStep)
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
        GarminConnectExecutableStepResponse workoutStep,
        int garminConnectFtp)
    {
        var (from, to) = (workoutStep.TargetValueOne, workoutStep.TargetValueTwo) switch
        {
            ({ } fromValue, { } toValue) => (
                (int)Math.Round(fromValue / garminConnectFtp * 100),
                (int)Math.Round(toValue / garminConnectFtp * 100)
            ),
            _ => throw new ArgumentException($"{nameof(workoutStep.TargetValueOne)} and {nameof(workoutStep.TargetValueTwo)} must be set"),
        };

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
                // ramp for warmup and cooldown
                IsRampRange = GetWorkoutGroupType(workoutStep) is IntervalsIcuWorkoutGroupType.Warmup or IntervalsIcuWorkoutGroupType.Cooldown,
            }
        };
    }

    private static IntervalsIcuWorkoutGroupType GetWorkoutGroupType(
       GarminConnectExecutableStepResponse workoutStep)
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
