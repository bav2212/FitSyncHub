namespace FitSyncHub.GarminConnect.Models.Responses.TrainingPlan;

public record GarminTrainingPlanTaskWorkoutResponse
{
    public required Guid WorkoutUuid { get; init; }
    public required GarminTrainingPlanSportTypeResponse SportType { get; init; }
    public required string AdaptiveCoachingWorkoutStatus { get; init; }
}
