namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

public record WorkoutEndConditionResponse
{
    public int ConditionTypeId { get; init; }
    public string ConditionTypeKey { get; init; } = default!;
    public int DisplayOrder { get; init; }
    public bool Displayable { get; init; }
}
