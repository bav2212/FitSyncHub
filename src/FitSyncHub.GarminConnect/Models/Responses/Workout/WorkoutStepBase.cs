using System.Text.Json.Serialization;

namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(WorkoutExecutableStepResponse), "ExecutableStepDTO")]
[JsonDerivedType(typeof(WorkoutRepeatGroupResponse), "RepeatGroupDTO")]
public abstract record WorkoutStepBase
{
    public int? StepOrder { get; init; }
    public WorkoutStepTypeResponse StepType { get; init; } = default!;
    public WorkoutEndConditionResponse? EndCondition { get; init; }
    public double? EndConditionValue { get; init; }
}
public record WorkoutExecutableStepResponse : WorkoutStepBase
{
    public WorkoutStepTargetTypeResponse? TargetType { get; init; }
    public double? TargetValueOne { get; init; }
    public double? TargetValueTwo { get; init; }
}

public record WorkoutRepeatGroupResponse : WorkoutStepBase
{
    public int NumberOfIterations { get; init; }
    public List<WorkoutStepBase> WorkoutSteps { get; init; } = [];
}
