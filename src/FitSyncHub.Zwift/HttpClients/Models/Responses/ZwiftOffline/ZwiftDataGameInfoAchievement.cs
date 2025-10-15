namespace FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftOffline;

public record ZwiftDataGameInfoAchievement
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string ImageUrl { get; init; }
}
