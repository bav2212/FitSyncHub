namespace StravaWebhooksAzureFunctions.Data.Entities;

public class UserSession
{
#pragma warning disable IDE1006 // Naming Styles
    public required string id { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    public required string AuthenticityToken { get; set; }
    public required string CookiesCollectionRawData { get; set; }
    public required string AthleteUserName { get; set; }
}
