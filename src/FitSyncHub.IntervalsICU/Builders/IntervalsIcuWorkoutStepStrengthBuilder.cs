using FitSyncHub.Common.Workouts;

namespace FitSyncHub.IntervalsICU.Builders;

public class IntervalsIcuWorkoutStepStrengthBuilder : IntervalsIcuWorkoutStepBuilder
{
    private readonly StrengthWorkoutStep _workoutStep;

    public IntervalsIcuWorkoutStepStrengthBuilder(StrengthWorkoutStep workoutStep)
    {
        _workoutStep = workoutStep;
    }
    public override string Build()
    {
        AppendTimeSegment(_workoutStep.Time);

        if (StringBuilder[^1] != ' ')
        {
            StringBuilder.Append(' ');
        }

        if (_workoutStep.Type == WorkoutStepType.Rest)
        {
            return base.Build();
        }

        StringBuilder.Append('"');
        StringBuilder.Append(_workoutStep.ExerciseDisplayName);

        if (_workoutStep.Category is not null)
        {
            StringBuilder.Append($", Category: {_workoutStep.Category}");
        }

        if (_workoutStep.ExerciseName is not null)
        {
            StringBuilder.Append($", Name: {_workoutStep.ExerciseName}");
        }

        StringBuilder.Append('"');

        return base.Build();
    }
}
