namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

public record GarminWorkoutSegmentResponse
{
    public int SegmentOrder { get; init; }
    public GarminConnectSportTypeResponse SportType { get; init; } = default!;
    public List<GarminConnectWorkoutStepBase> WorkoutSteps { get; init; } = [];
}
