namespace FitSyncHub.Strava.HttpClients.Models.Responses.Activities;

public sealed record ActivityAchievement
{
    public int? TypeId { get; init; }
    public string? Type { get; init; }
    public int? Rank { get; init; }
}
