namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

public sealed record GarminWorkoutStepTargetTypeResponse
{
    public int WorkoutTargetTypeId { get; init; }
    public string WorkoutTargetTypeKey { get; init; } = default!;
    public int DisplayOrder { get; init; }
}
