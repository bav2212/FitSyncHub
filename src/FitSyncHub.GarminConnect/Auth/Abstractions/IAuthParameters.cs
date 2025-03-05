namespace FitSyncHub.GarminConnect.Auth.Abstractions;

internal interface IAuthParameters
{
    string UserAgent { get; }
    string Domain { get; }
    string? Cookies { get; set; }
    string? Csrf { get; set; }

    IReadOnlyDictionary<string, string> GetHeaders();
    IReadOnlyDictionary<string, string> GetFormParameters();
    IReadOnlyDictionary<string, string> GetQueryParameters();
}
