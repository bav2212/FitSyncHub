using FitSyncHub.Functions.Data.Entities.Abstractions;

namespace FitSyncHub.Functions.Data.Entities;

public class UserSession : DataModel
{
    public required string AuthenticityToken { get; set; }
    public required string CookiesCollectionRawData { get; set; }
    public required string AthleteUserName { get; set; }
}
