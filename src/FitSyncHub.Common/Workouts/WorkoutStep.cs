namespace FitSyncHub.Common.Workouts;

public abstract record WorkoutStep
{
    public required WorkoutStepType Type { get; init; }
}

public record RepeatableWorkoutStep : WorkoutStep
{
    public required int NumberOfIterations { get; init; }
    public required List<WorkoutStep> Items { get; init; }
}

public abstract record ExecutableWorkoutStep : WorkoutStep
{
    public required TimeSpan Time { get; init; }
}

public record StrengthWorkoutStep : ExecutableWorkoutStep
{
    public required string ExerciseDisplayName { get; init; }
    public string? Category { get; init; }
    public string? ExerciseName { get; init; }
}

public record RideWorkoutStep : ExecutableWorkoutStep
{
    public int? Rpm { get; init; }
    public IRideFtp? Ftp { get; init; }
    public bool IsMaxEffort { get; init; }
    public bool IsFreeRide { get; init; }
}
