namespace FitSyncHub.GarminConnect.Models.Responses.TrainingPlan;

public record TrainingPlanResponse
{
    public required List<TrainingPlanTaskItemResponse> TaskList { get; init; }
}
