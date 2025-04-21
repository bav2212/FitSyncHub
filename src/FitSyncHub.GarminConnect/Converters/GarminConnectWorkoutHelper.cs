using System.Text.RegularExpressions;
using FitSyncHub.GarminConnect.Models.Responses.Workout;

namespace FitSyncHub.GarminConnect.Converters;

public static partial class GarminConnectWorkoutHelper
{
    public static string GetWorkoutTitle(WorkoutResponse workoutResponse, int ftp)
    {
        return (workoutResponse.WorkoutName, workoutResponse.Description) switch
        {
            ({ } workoutName, { } description) => $"{workoutName} {GetWorkoutDescription(description, ftp)}",
            ({ } workoutName, null) => workoutName,
            _ => throw new ArgumentException("Workout name and description cannot be null")
        };
    }
    internal static IEnumerable<WorkoutExecutableStepResponse> GetFlattenWorkoutSteps(
        WorkoutRepeatGroupResponse repeatStepRoot)
    {
        foreach (var step in repeatStepRoot.WorkoutSteps)
        {
            if (step is WorkoutRepeatGroupResponse repeatStep)
            {
                // flatten the repeatable step, cause intervals.icu does not support nested repeatable steps
                foreach (var item in Enumerable
                    .Repeat(GetFlattenWorkoutSteps(repeatStep), repeatStep.NumberOfIterations)
                    .SelectMany(x => x))
                {
                    yield return item;
                }
            }

            if (step is WorkoutExecutableStepResponse singleStep)
            {
                yield return singleStep;
            }
        }
    }

    internal static int GetRoundedPercent(double watts, int ftp)
    {
        var rawPercent = (double)watts / ftp * 100;
        return (int)(Math.Round(rawPercent / 5.0) * 5);
    }

    [GeneratedRegex(@"(?<prefix>@?)(?<watts>\d+)[wW]")]
    private static partial Regex AbsoluteWattsRegexPattern();

    private static string GetWorkoutDescription(string description, int ftp)
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

            description = absoluteWattsRegexPattern.Replace(description, newText);
        }

        return description;
    }
}
