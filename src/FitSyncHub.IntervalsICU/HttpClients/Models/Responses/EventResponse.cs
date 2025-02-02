namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public class EventResponse
{
    public required int Id { get; set; }
    public required string StartDateLocal { get; set; }
    public required string Type { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required bool Indoor { get; set; }
    public required List<string> Tags { get; set; }
}
