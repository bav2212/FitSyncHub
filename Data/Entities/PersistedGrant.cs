namespace StravaWebhooksAzureFunctions.Data.Entities;

public class PersistedGrant
{
#pragma warning disable IDE1006 // Naming Styles
    public required string id { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    public required string TokenType { get; set; }
    public required long ExpiresAt { get; set; }
    public required long ExpiresIn { get; set; }
    public required string RefreshToken { get; set; }
    public required string AccessToken { get; set; }
    public required long AthleteId { get; set; }
    public required string AthleteUserName { get; set; }
}
