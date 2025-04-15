namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

public record WorkoutResponse
{
    public long? WorkoutId { get; init; }
    public long OwnerId { get; init; }
    public required string WorkoutName { get; init; }
    public string? Description { get; init; }
    public DateTime? CreatedDate { get; init; }
    public WorkoutSportTypeResponse SportType { get; init; } = default!;
    public string SubSportType { get; init; } = default!;
    public int? EstimatedDurationInSecs { get; init; }
    public List<WorkoutSegmentResponse> WorkoutSegments { get; init; } = [];
}
