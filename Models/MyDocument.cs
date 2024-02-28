namespace StravaWebhooksAzureFunctions.Models;

public struct MyDocument
{
#pragma warning disable IDE1006 // Naming Styles
    public required string id { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    public required string Text { get; set; }
    public required int Number { get; set; }
    public required bool Boolean { get; set; }
}
