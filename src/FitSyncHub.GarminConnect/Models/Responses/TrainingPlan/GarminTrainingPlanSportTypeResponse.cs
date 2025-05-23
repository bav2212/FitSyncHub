namespace FitSyncHub.GarminConnect.Models.Responses.TrainingPlan;

public record GarminTrainingPlanSportTypeResponse
{
    public required string? SportTypeKey { get; init; }
}
