namespace FitSyncHub.Zwift.HttpClients.Models.Requests.Profiles;

public sealed record ZwiftSearchProfileRequest
{
    public required string SearchText { get; set; }
    public int PageLimit { get; set; } = 1;
    public int Limit { get; set; } = 50;
    public int Start { get; set; } = 0;
}

