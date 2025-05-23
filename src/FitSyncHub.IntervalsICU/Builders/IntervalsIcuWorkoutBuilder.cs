using System.Text;
using FitSyncHub.Common.Workouts;

namespace FitSyncHub.IntervalsICU.Builders;

public class IntervalsIcuWorkoutBuilder
{
    private readonly StringBuilder _sb;
    private bool _skipDoubledRecoverySteps;

    public IntervalsIcuWorkoutBuilder()
    {
        _sb = new StringBuilder();
    }

    public IntervalsIcuWorkoutBuilder WithSkipDoubledRecovery()
    {
        _skipDoubledRecoverySteps = true;
        return this;
    }

    public string Build(Workout workout)
    {
        var workoutSteps = workout.Steps;

        return _skipDoubledRecoverySteps && workout.Type == WorkoutType.Ride
            ? BuildWorkoutAndSkipDoubledRecoverySteps(workoutSteps)
            : BuildWorkout(workoutSteps);
    }

    private string BuildWorkout(IReadOnlyCollection<WorkoutStep> workoutSteps)
    {
        foreach (var workoutStep in workoutSteps)
        {
            if (workoutStep is ExecutableWorkoutStep singleStep)
            {
                _sb.AppendLine(string.Empty);
                _sb.AppendLine($"{singleStep.Type}");
                _sb.AppendLine(GetSingleStepBuilder(singleStep).Build());
            }

            if (workoutStep is RepeatableWorkoutStep repeatableStep)
            {
                _sb.AppendLine(string.Empty);
                List<ExecutableWorkoutStep> rideSteps = [];
                foreach (var item in repeatableStep.Items)
                {
                    if (item is RepeatableWorkoutStep a)
                    {
                        rideSteps.AddRange([.. FlattenSteps(a)]);
                    }
                    else if (item is ExecutableWorkoutStep b)
                    {
                        rideSteps.Add(b);
                    }
                }

                _sb.AppendLine($"{repeatableStep.NumberOfIterations}x");

                foreach (var item in rideSteps)
                {
                    _sb.AppendLine(GetSingleStepBuilder(item).Build());
                }
            }
        }

        return _sb.ToString();
    }

    private string BuildWorkoutAndSkipDoubledRecoverySteps(IReadOnlyCollection<WorkoutStep> workoutSteps)
    {
        foreach (var singleStep in FlattenStepsAndRemoveDoubleRecoveries(workoutSteps).ToList())
        {
            if (singleStep.Type is WorkoutStepType.Cooldown)
            {
                _sb.AppendLine(string.Empty);
            }

            if (singleStep.Type is WorkoutStepType.Warmup or WorkoutStepType.Cooldown)
            {
                _sb.AppendLine($"{singleStep.Type}");
            }

            _sb.AppendLine(GetSingleStepBuilder(singleStep).Build());

            if (singleStep.Type is WorkoutStepType.Warmup)
            {
                _sb.AppendLine(string.Empty);
            }
        }

        return _sb.ToString();
    }

    private static IEnumerable<ExecutableWorkoutStep> FlattenStepsAndRemoveDoubleRecoveries(
        IReadOnlyCollection<WorkoutStep> workoutSteps)
    {
        var flatten = workoutSteps.SelectMany(FlattenSteps);
        if (!flatten.Any())
        {
            yield break;
        }

        ExecutableWorkoutStep? previous = default;

        foreach (var step in flatten)
        {
            if (previous?.Type is WorkoutStepType.Recovery &&
                step.Type is WorkoutStepType.Recovery or WorkoutStepType.Cooldown or WorkoutStepType.Rest)
            {
                // skip doubled recovery
                previous = step;
                continue;
            }

            if (previous is not null)
            {
                yield return previous;
            }

            previous = step;
        }

        // last one
        yield return previous!;
    }

    private static IEnumerable<ExecutableWorkoutStep> FlattenSteps(WorkoutStep workoutStep)
    {
        if (workoutStep is ExecutableWorkoutStep singleStep)
        {
            yield return singleStep;
        }

        if (workoutStep is RepeatableWorkoutStep repeatableStep)
        {
            foreach (var repetition in Enumerable.Range(0, repeatableStep.NumberOfIterations))
            {
                foreach (var step in repeatableStep.Items.SelectMany(FlattenSteps))
                {
                    yield return step;
                }
            }
        }
    }

    private static IntervalsIcuWorkoutStepBuilder GetSingleStepBuilder(WorkoutStep item)
    {
        return item switch
        {
            RideWorkoutStep rideStep => new IntervalsIcuWorkoutStepRideBuilder(rideStep),
            StrengthWorkoutStep strengthStep => new IntervalsIcuWorkoutStepStrengthBuilder(strengthStep),
            _ => throw new NotImplementedException(),
        };
    }
}
