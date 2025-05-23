namespace FitSyncHub.Common.Workouts;

public record Workout
{
    public required WorkoutType Type { get; init; }
    public required List<WorkoutStep> Steps { get; init; }
}
