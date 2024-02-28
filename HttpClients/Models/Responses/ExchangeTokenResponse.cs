using System;
using System.Text.Json.Serialization;

namespace StravaWebhooksAzureFunctions.HttpClients.Models.Responses;

public class ExchangeTokenResponse
{
    [JsonPropertyName("token_type")]
    public string TokenType { get; init; }
    [JsonPropertyName("expires_at")]
    public int ExpiresAt { get; init; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; }
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }
    [JsonPropertyName("athlete")]
    public Athlete Athlete { get; init; }
}

public record Athlete
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    [JsonPropertyName("username")]
    public string Username { get; init; }
    [JsonPropertyName("resource_state")]
    public int ResourceState { get; init; }
    [JsonPropertyName("firstname")]
    public string Firstname { get; init; }
    [JsonPropertyName("lastname")]
    public string Lastname { get; init; }
    [JsonPropertyName("bio")]
    public string Bio { get; init; }
    [JsonPropertyName("city")]
    public string City { get; init; }
    [JsonPropertyName("state")]
    public string State { get; init; }
    [JsonPropertyName("country")]
    public string Country { get; init; }
    [JsonPropertyName("sex")]
    public string Sex { get; init; }
    [JsonPropertyName("premium")]
    public bool Premium { get; init; }
    [JsonPropertyName("summit")]
    public bool Summit { get; init; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }
    [JsonPropertyName("badge_type_id")]
    public int BadgeTypeId { get; init; }
    [JsonPropertyName("weight")]
    public float Weight { get; init; }
    [JsonPropertyName("profile_medium")]
    public string ProfileMedium { get; init; }
    [JsonPropertyName("profile")]
    public string Profile { get; init; }
    [JsonPropertyName("friend")]
    public string Friend { get; init; }
    [JsonPropertyName("follower")]
    public string Follower { get; init; }
}
