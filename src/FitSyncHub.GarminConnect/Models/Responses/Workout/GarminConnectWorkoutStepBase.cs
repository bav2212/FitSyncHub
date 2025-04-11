using System.Text.Json.Serialization;

namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(GarminConnectExecutableStepResponse), "ExecutableStepDTO")]
[JsonDerivedType(typeof(GarminConnectRepeatGroupResponse), "RepeatGroupDTO")]
public abstract record GarminConnectWorkoutStepBase
{
    public int? StepOrder { get; init; }
    public GarminConnectStepTypeResponse StepType { get; init; } = default!;
    public GarminConnectEndConditionResponse? EndCondition { get; init; }
    public double? EndConditionValue { get; init; }
}
public record GarminConnectExecutableStepResponse : GarminConnectWorkoutStepBase
{
    public GarminConnectTargetTypeResponse? TargetType { get; init; }
    public double? TargetValueOne { get; init; }
    public double? TargetValueTwo { get; init; }
}

public record GarminConnectRepeatGroupResponse : GarminConnectWorkoutStepBase
{
    public int NumberOfIterations { get; init; }
    public List<GarminConnectWorkoutStepBase> WorkoutSteps { get; init; } = [];
}
