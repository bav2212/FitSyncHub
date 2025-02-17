using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace FitSyncHub.Strava.Models.Responses;

public record CookieLoginResponse
{
    [MemberNotNullWhen(true, nameof(Cookies))]
    [MemberNotNullWhen(true, nameof(AuthenticityToken))]
    public required bool Success { get; init; }

    public required CookieContainer? Cookies { get; init; }
    public required string? AuthenticityToken { get; init; }
}
