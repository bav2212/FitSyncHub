namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public record AddMessageRequest
{
    public required string Content { get; init; }
}
