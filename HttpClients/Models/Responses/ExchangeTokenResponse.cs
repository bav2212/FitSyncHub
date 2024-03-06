namespace StravaWebhooksAzureFunctions.HttpClients.Models.Responses;

public record ExchangeTokenResponse
{
    public required string TokenType { get; init; }
    public required int ExpiresAt { get; init; }
    public required int ExpiresIn { get; init; }
    public required string RefreshToken { get; init; }
    public required string AccessToken { get; init; }
    public required Athlete Athlete { get; init; }
}

public record Athlete
{
    public required int Id { get; init; }
    public required string Username { get; init; }
    public required int ResourceState { get; init; }
    public required string Firstname { get; init; }
    public required string Lastname { get; init; }
    public required string Bio { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string Country { get; init; }
    public required string Sex { get; init; }
    public required bool Premium { get; init; }
    public required bool Summit { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required int BadgeTypeId { get; init; }
    public required float Weight { get; init; }
    public required string ProfileMedium { get; init; }
    public required string Profile { get; init; }
    public required string Friend { get; init; }
    public required string Follower { get; init; }
}
