using FitSyncHub.Functions.Data.Entities.Abstractions;

namespace FitSyncHub.Functions.Data.Entities;

public class PersistedGrant : DataModel
{
    public required string TokenType { get; set; }
    public required long ExpiresAt { get; set; }
    public required long ExpiresIn { get; set; }
    public required string RefreshToken { get; set; }
    public required string AccessToken { get; set; }
    public required long AthleteId { get; set; }
    public required string AthleteUserName { get; set; }
}
