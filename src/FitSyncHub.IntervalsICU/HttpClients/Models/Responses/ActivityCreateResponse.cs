namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public record ActivityCreateResponse
{
    public required string IcuAthleteId { get; init; }
    public required string Id { get; init; }
}
