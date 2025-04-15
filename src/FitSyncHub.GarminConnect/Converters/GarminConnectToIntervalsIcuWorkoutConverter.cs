using System.Text.RegularExpressions;
using FitSyncHub.Common.Applications.IntervalsIcu.Models;
using FitSyncHub.GarminConnect.Models.Responses.Workout;

namespace FitSyncHub.GarminConnect.Converters;

public static partial class GarminConnectToIntervalsIcuWorkoutConverter
{
    public static string GetWorkoutDescription(WorkoutResponse workoutResponse, int ftp)
    {
        var name = workoutResponse.Description;

        // Capture optional @ as group "prefix" and watt number as group "watts"
        var absoluteWattsRegexPattern = AbsoluteWattsRegexPattern();
        var match = AbsoluteWattsRegexPattern().Match(name);

        if (match.Success)
        {
            var watts = int.Parse(match.Groups["watts"].Value);
            var rawPercent = (double)watts / ftp * 100;
            var roundedPercent = (int)(Math.Round(rawPercent / 5.0) * 5);

            var prefix = match.Groups["prefix"].Value;
            var newText = $"{prefix}{roundedPercent}%";

            name = absoluteWattsRegexPattern.Replace(name, newText, 1); // Replace only the first match
        }

        return name;
    }

    [GeneratedRegex(@"(?<prefix>@?)(?<watts>\d+)[wW]")]
    private static partial Regex AbsoluteWattsRegexPattern();

    public static List<IntervalsIcuWorkoutGroup> ConvertGarminWorkoutToIntervalsIcuWorkoutGroups(
        WorkoutResponse workout, int garminConnectFtp)
    {
        List<IntervalsIcuWorkoutGroup> result = [];

        foreach (var step in workout.WorkoutSegments[0].WorkoutSteps)
        {
            if (step is WorkoutExecutableStepResponse workoutStep)
            {
                var item = GetIntervalsIcuWorkoutLine(workoutStep, garminConnectFtp);

                var group = new IntervalsIcuWorkoutGroup
                {
                    BlockInfo = CreateBlockInfo(workoutStep),
                    Items = [item]
                };
                result.Add(group);
            }

            if (step is WorkoutRepeatGroupResponse repeatStep)
            {
                var group = new IntervalsIcuWorkoutGroup
                {
                    BlockInfo = IntervalsIcuWorkoutGroupBlockInfo.CreateInterval(repeatStep.NumberOfIterations),
                    Items = [.. repeatStep.WorkoutSteps
                        .OfType<WorkoutExecutableStepResponse>()
                        .Select(x => GetIntervalsIcuWorkoutLine(x, garminConnectFtp))]
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
        int garminConnectFtp)
    {
        var workoutGroupType = GetWorkoutGroupType(workoutStep);

        var (from, to) = (workoutStep.TargetValueOne, workoutStep.TargetValueTwo) switch
        {
            ({ } fromValue, { } toValue) => (
                (int)Math.Round(fromValue / garminConnectFtp * 100),
                (int)Math.Round(toValue / garminConnectFtp * 100)
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
                // ramp for warmup and cooldown
                IsRampRange = workoutGroupType is IntervalsIcuWorkoutGroupType.Warmup or IntervalsIcuWorkoutGroupType.Cooldown,
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
