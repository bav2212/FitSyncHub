namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public sealed record MessageAddRequest
{
    public required string Content { get; init; }
}
