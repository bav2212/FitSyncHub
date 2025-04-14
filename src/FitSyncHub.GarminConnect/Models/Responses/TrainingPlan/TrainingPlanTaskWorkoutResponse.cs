namespace FitSyncHub.GarminConnect.Models.Responses.TrainingPlan;

public record TrainingPlanTaskWorkoutResponse
{
    public required Guid WorkoutUuid { get; init; }
    public required TrainingPlanSportTypeResponse SportType { get; init; }
}
