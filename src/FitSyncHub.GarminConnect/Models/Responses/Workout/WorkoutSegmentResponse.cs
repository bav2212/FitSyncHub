namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

public record WorkoutSegmentResponse
{
    public int SegmentOrder { get; init; }
    public WorkoutSportTypeResponse SportType { get; init; } = default!;
    public List<WorkoutStepBase> WorkoutSteps { get; init; } = [];
}
