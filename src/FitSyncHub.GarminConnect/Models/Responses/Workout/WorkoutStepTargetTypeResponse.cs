namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

public record WorkoutStepTargetTypeResponse
{
    public int WorkoutTargetTypeId { get; init; }
    public string WorkoutTargetTypeKey { get; init; } = default!;
    public int DisplayOrder { get; init; }
}
