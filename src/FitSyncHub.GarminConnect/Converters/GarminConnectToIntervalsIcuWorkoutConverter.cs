using System.Text.RegularExpressions;
using FitSyncHub.Common.Applications.IntervalsIcu.Models;
using FitSyncHub.GarminConnect.Models.Responses.Workout;

namespace FitSyncHub.GarminConnect.Converters;

public static partial class GarminConnectToIntervalsIcuWorkoutConverter
{
    public static string GetGarminWorkoutTitle(WorkoutResponse workoutResponse, int ftp)
    {
        return (workoutResponse.WorkoutName, workoutResponse.Description) switch
        {
            ({ } workoutName, { } description) => $"{workoutName} {GetDescription(ftp, description)}",
            ({ } workoutName, null) => workoutName,
            _ => throw new ArgumentException("Workout name and description cannot be null")
        };
    }

    private static string GetDescription(int ftp, string description)
    {
        // Capture optional @ as group "prefix" and watt number as group "watts"
        var absoluteWattsRegexPattern = AbsoluteWattsRegexPattern();
        var match = AbsoluteWattsRegexPattern().Match(description);

        if (match.Success)
        {
            var watts = int.Parse(match.Groups["watts"].Value);
            var roundedPercent = GetRoundedPercent(watts, ftp);

            var prefix = match.Groups["prefix"].Value;
            var newText = $"{prefix}{roundedPercent}%";

            description = absoluteWattsRegexPattern.Replace(description, newText, 1); // Replace only the first match
        }

        return description;
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
                    Items = [.. GetIntervalsIcuWorkoutLineForRepeatableStep(garminConnectFtp, repeatStep)]
                };
                result.Add(group);
            }
        }

        return result;
    }

    private static IEnumerable<IntervalsIcuWorkoutLine> GetIntervalsIcuWorkoutLineForRepeatableStep(
        int garminConnectFtp, WorkoutRepeatGroupResponse repeatStepRoot)
    {
        foreach (var step in repeatStepRoot.WorkoutSteps)
        {
            if (step is WorkoutRepeatGroupResponse repeatStep)
            {
                // flatten the repeatable step, cause intervals.icu does not support nested repeatable steps
                foreach (var item in Enumerable
                    .Repeat(GetIntervalsIcuWorkoutLineForRepeatableStep(garminConnectFtp, repeatStep), repeatStep.NumberOfIterations)
                    .SelectMany(x => x))
                {
                    yield return item;
                }
            }

            if (step is WorkoutExecutableStepResponse singleStep)
            {
                yield return GetIntervalsIcuWorkoutLine(singleStep, garminConnectFtp);
            }
        }
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
                GetRoundedPercent(fromValue, garminConnectFtp),
                GetRoundedPercent(toValue, garminConnectFtp)
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

    private static int GetRoundedPercent(double watts, int ftp)
    {
        var rawPercent = (double)watts / ftp * 100;
        return (int)(Math.Round(rawPercent / 5.0) * 5);
    }
}
