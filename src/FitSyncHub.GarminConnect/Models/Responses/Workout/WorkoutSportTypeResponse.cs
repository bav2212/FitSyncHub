namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

public record WorkoutSportTypeResponse
{
    public int SportTypeId { get; init; }
    public string SportTypeKey { get; init; } = default!;
    public int DisplayOrder { get; init; }
}
