namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

public sealed record GarminWorkoutStepTypeResponse
{
    public int StepTypeId { get; init; }
    public string StepTypeKey { get; init; } = default!;
    public int DisplayOrder { get; init; }
}
