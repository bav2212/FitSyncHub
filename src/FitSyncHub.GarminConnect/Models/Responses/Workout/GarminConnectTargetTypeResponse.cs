namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

public record GarminConnectTargetTypeResponse
{
    public int WorkoutTargetTypeId { get; init; }
    public string WorkoutTargetTypeKey { get; init; } = default!;
    public int DisplayOrder { get; init; }
}
