namespace StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Athletes;

public class SummaryGearResponse
{
    public string Id { get; init; }
    public int ResourceState { get; init; }
    public bool Primary { get; init; }
    public string? Name { get; init; }
    public float Distance { get; init; }
}