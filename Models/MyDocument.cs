namespace StravaWebhooksAzureFunctions.Models;

public struct MyDocument
{
    public required string id { get; set; }
    public required string Text { get; set; }
    public required int Number { get; set; }
    public required bool Boolean { get; set; }
}
