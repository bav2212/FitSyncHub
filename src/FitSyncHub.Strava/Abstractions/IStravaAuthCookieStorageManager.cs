namespace FitSyncHub.Strava.Abstractions;

public interface IStravaAuthCookieStorageManager
{
    Task<SerializedCookieTuple?> ReadCookies(
        string username,
        CancellationToken cancellationToken);

    Task StoreCookies(
        string username,
        string serializedCookies,
        string authenticityToken,
        CancellationToken cancellationToken);

    Task DeleteCookies(
       string username,
       CancellationToken cancellationToken);
}

public record SerializedCookieTuple
{
    public required string SerializedCookies { get; init; }
    public required string AuthenticityToken { get; init; }
}
