namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

public record GarminConnectWorkoutSegmentResponse
{
    public int SegmentOrder { get; init; }
    public GarminConnectSportTypeResponse SportType { get; init; } = default!;
    public List<GarminConnectWorkoutStepBase> WorkoutSteps { get; init; } = [];
}
