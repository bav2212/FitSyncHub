namespace StravaWebhooksAzureFunctions.Data.Entities;

public class UserSession
{
    public int Id { get; set; }
    public string AuthenticityToken { get; set; }
    public string CookiesCollectionRawData { get; set; }
    public string AthleteUserName { get; set; }
}
