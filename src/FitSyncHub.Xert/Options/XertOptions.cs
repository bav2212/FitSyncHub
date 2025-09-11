namespace FitSyncHub.Xert.Options;

public record XertOptions
{
    public required XertAuthOptions Credentials { get; init; }

    public record XertAuthOptions
    {
        public required string Username { get; init; }
        public required string Password { get; init; }
    }
}
