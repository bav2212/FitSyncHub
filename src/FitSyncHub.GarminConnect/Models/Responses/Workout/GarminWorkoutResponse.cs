namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

public sealed record GarminWorkoutResponse
{
    public long? WorkoutId { get; init; }
    public long OwnerId { get; init; }
    public required string WorkoutName { get; init; }
    public string? Description { get; init; }
    public DateTime? CreatedDate { get; init; }
    public GarminWorkoutSportTypeResponse SportType { get; init; } = default!;
    public string SubSportType { get; init; } = default!;
    public int? EstimatedDurationInSecs { get; init; }
    public List<GarminWorkoutSegmentResponse> WorkoutSegments { get; init; } = [];
}
