using FitSyncHub.Common.Applications.IntervalsIcu.Models;
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

    public IIntervalsIcuWorkoutLine? Convert(WorkoutExecutableStepResponse workoutStep)
    {
        if (workoutStep.StepType.StepTypeKey == "rest")
        {
            return default;
        }

        var secondsValue = workoutStep.EndConditionValue
            ?? throw new ArgumentException($"{nameof(workoutStep.EndConditionValue)} must be set");
        var time = TimeSpan.FromSeconds(secondsValue);

        if (workoutStep.Category is null || workoutStep.ExerciseName is null)
        {
            throw new InvalidOperationException($"{nameof(workoutStep.ExerciseName)} must be set");
        }

        var translationKey = $"{workoutStep.Category}_{workoutStep.ExerciseName}";
        return new IntervalsIcuStrengthWorkoutLine()
        {
            Time = time,
            ExerciseName = _exerciseNameTranslations.GetValueOrDefault(translationKey, workoutStep.ExerciseName),
        };
    }
}
