namespace FitSyncHub.Common;

public interface IStravaApplicationOptionsProvider
{
    string ClientId { get; }
    string ClientSecret { get; }
}
