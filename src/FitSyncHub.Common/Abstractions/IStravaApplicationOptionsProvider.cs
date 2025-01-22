namespace FitSyncHub.Common.Abstractions;

public interface IStravaApplicationOptionsProvider
{
    string ClientId { get; }
    string ClientSecret { get; }
}
