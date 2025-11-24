namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public sealed record ActivityCreateResponse
{
    public required string IcuAthleteId { get; init; }
    public required string Id { get; init; }
}
