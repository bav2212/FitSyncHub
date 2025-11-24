namespace FitSyncHub.Zwift.Options;

public sealed record ZwiftAuthOptions
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}
