namespace FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;

public record ZwiftGameInfoAchievement
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string ImageUrl { get; init; }
}
