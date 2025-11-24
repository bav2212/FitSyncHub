namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

public sealed record GarminWorkoutSegmentResponse
{
    public int SegmentOrder { get; init; }
    public GarminWorkoutSportTypeResponse SportType { get; init; } = default!;
    public List<GarminWorkoutStepBase> WorkoutSteps { get; init; } = [];
}
