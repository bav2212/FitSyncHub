namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

public record GarminConnectWorkoutResponse
{
    public long? WorkoutId { get; init; }
    public long OwnerId { get; init; }
    public string WorkoutName { get; init; } = default!;
    public string Description { get; init; } = default!;
    public DateTime? CreatedDate { get; init; }
    public GarminConnectSportTypeResponse SportType { get; init; } = default!;
    public string SubSportType { get; init; } = default!;
    public int? EstimatedDurationInSecs { get; init; }
    public List<GarminConnectWorkoutSegmentResponse> WorkoutSegments { get; init; } = [];
}
