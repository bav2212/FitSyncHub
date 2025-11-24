namespace FitSyncHub.Strava.Models.Requests;

public sealed record ExchangeTokenRequest
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string Code { get; init; }
    public required string GrantType { get; init; }
}
