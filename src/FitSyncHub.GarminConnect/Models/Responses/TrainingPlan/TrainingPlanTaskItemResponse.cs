namespace FitSyncHub.GarminConnect.Models.Responses.TrainingPlan;

public record TrainingPlanTaskItemResponse
{
    public required DateOnly CalendarDate { get; init; }
    public required TrainingPlanTaskWorkoutResponse TaskWorkout { get; init; }
}
