namespace FitSyncHub.Zwift.HttpClients.Models.Responses.ZwiftOffline;

public record ZwiftDataGameInfoResponse
{
    public required List<ZwiftDataGameInfoAchievement> Achievements { get; init; }
    public required List<ZwiftDataGameInfoMap> Maps { get; init; }
}
