namespace FitSyncHub.GarminConnect.Models.Responses.TrainingPlan;

public record TrainingPlanTaskWorkoutResponse
{
    public required Guid WorkoutUuid { get; init; }
    public required TrainingPlanSportTypeResponse SportType { get; init; }
    public required string AdaptiveCoachingWorkoutStatus { get; init; }
}
