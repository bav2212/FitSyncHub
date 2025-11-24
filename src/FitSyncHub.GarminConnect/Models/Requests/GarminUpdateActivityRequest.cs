namespace FitSyncHub.GarminConnect.Models.Requests;

public sealed record GarminActivityUpdateRequest
{
    public required long ActivityId { get; set; }
    public required string? ActivityName { get; set; }
    public required GarminActivityUpdateSummary SummaryDTO { get; set; }
    public required string? Description { get; set; }
}

public sealed record GarminActivityUpdateSummary
{
    public required int Distance { get; set; }
    public required int ElevationGain { get; set; }
}
