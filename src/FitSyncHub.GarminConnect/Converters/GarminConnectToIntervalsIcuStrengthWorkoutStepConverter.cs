using FitSyncHub.Common.Workouts;
using FitSyncHub.GarminConnect.Models.Responses.Workout;

namespace FitSyncHub.GarminConnect.Converters;

internal class GarminConnectToIntervalsIcuStrengthWorkoutStepConverter
    : IGarminConnectToIntervalsIcuWorkoutStepConverter
{
    private readonly Dictionary<string, string> _exerciseNameTranslations;

    public GarminConnectToIntervalsIcuStrengthWorkoutStepConverter(
        Dictionary<string, string> exerciseNameTranslations)
    {
        _exerciseNameTranslations = exerciseNameTranslations;
    }

    public ExecutableWorkoutStep? Convert(GarminWorkoutExecutableStepResponse workoutStep)
    {
        var stepType = GarminConnectWorkoutHelper.GetStepType(workoutStep);
        var secondsValue = workoutStep.EndConditionValue
            ?? throw new ArgumentException($"{nameof(workoutStep.EndConditionValue)} must be set");
        var time = TimeSpan.FromSeconds(secondsValue);

        if (stepType == WorkoutStepType.Rest)
        {
            return new StrengthWorkoutStep
            {
                Type = stepType,
                Time = time,
                ExerciseDisplayName = "Rest"
            };
        }

        if (workoutStep.Category is null || workoutStep.ExerciseName is null)
        {
            throw new InvalidOperationException($"{nameof(workoutStep.ExerciseName)} must be set");
        }

        var translationKey = $"{workoutStep.Category}_{workoutStep.ExerciseName}";
        return new StrengthWorkoutStep
        {
            Type = stepType,
            Time = time,
            Category = workoutStep.Category,
            ExerciseName = workoutStep.ExerciseName,
            ExerciseDisplayName = _exerciseNameTranslations.GetValueOrDefault(translationKey, workoutStep.ExerciseName),
        };
    }
}
