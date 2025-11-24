using System.Text.Json.Serialization;

namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(GarminWorkoutExecutableStepResponse), "ExecutableStepDTO")]
[JsonDerivedType(typeof(GarminWorkoutRepeatGroupResponse), "RepeatGroupDTO")]
public abstract record GarminWorkoutStepBase
{
    public int? StepOrder { get; init; }
    public GarminWorkoutStepTypeResponse StepType { get; init; } = default!;
    public GarminWorkoutEndConditionResponse? EndCondition { get; init; }
    public double? EndConditionValue { get; init; }
}
public sealed record GarminWorkoutExecutableStepResponse : GarminWorkoutStepBase
{
    public GarminWorkoutStepTargetTypeResponse? TargetType { get; init; }
    public double? TargetValueOne { get; init; }
    public double? TargetValueTwo { get; init; }
    public string? Category { get; init; }
    public string? ExerciseName { get; init; }
}

public sealed record GarminWorkoutRepeatGroupResponse : GarminWorkoutStepBase
{
    public int NumberOfIterations { get; init; }
    public List<GarminWorkoutStepBase> WorkoutSteps { get; init; } = [];
}
