using System.Text.Json.Serialization;

namespace FitSyncHub.Functions.HttpClients.Models.Responses.Athletes;

public class DetailedAthleteResponse
{
    public long Id { get; init; }
    public int ResourceState { get; init; }
    [JsonPropertyName("firstname")]
    public string? FirstName { get; init; }
    [JsonPropertyName("lastname")]
    public string? LastName { get; init; }
    public string? ProfileMedium { get; init; }
    public string? Profile { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? Country { get; init; }
    public string? Sex { get; init; }
    public bool? Premium { get; init; }
    public bool? Summit { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int FollowerCount { get; init; }
    public int FriendCount { get; init; }
    public string? MeasurementPreference { get; init; }
    public int Ftp { get; init; }
    public float Weight { get; init; }
    //public SummaryClubResponse Clubs { get; init; }
    public List<SummaryGearResponse> Bikes { get; init; } = [];
    public List<SummaryGearResponse> Shoes { get; init; } = [];
}
