namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public record CreateEventFromDescriptionRequest : CreateEventRequestBase
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}
