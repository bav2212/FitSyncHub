namespace FitSyncHub.Zwift.HttpClients.Models.Responses.Events;

public record ZwiftEventResponse
{
    public DateTime EventStart { get; set; }
    public required ZwiftEventSubgroupResponse[] EventSubgroups { get; set; }
}

public record ZwiftEventSubgroupResponse
{
    public int Id { get; set; }
    public string? SubgroupLabel { get; set; }
    public string[]? RulesSet { get; set; }
}
