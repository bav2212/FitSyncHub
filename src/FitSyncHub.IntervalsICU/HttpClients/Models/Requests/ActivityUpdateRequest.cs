namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public class ActivityUpdateRequest
{
    public required bool? Commute { get; set; }
    public required bool? Race { get; set; }
    public required bool? Trainer { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Type { get; set; }
    public required GearUpdateRequest Gear { get; set; }
}


public class GearUpdateRequest
{
    public required string Id { get; set; }
}
