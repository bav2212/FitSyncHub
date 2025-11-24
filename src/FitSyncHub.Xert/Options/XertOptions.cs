namespace FitSyncHub.Xert.Options;

public sealed record XertOptions
{
    public required XertAuthOptions Credentials { get; init; }

    public sealed record XertAuthOptions
    {
        public required string Username { get; init; }
        public required string Password { get; init; }
    }
}
