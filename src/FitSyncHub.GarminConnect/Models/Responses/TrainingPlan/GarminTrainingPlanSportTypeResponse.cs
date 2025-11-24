namespace FitSyncHub.GarminConnect.Models.Responses.TrainingPlan;

public sealed record GarminTrainingPlanSportTypeResponse
{
    public required string? SportTypeKey { get; init; }
}
