using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace FitSyncHub.Strava.Models.Responses;

public record CookieLoginResponse
{
    public static readonly CookieLoginResponse Unsuccess = new(success: false);

    private CookieLoginResponse(bool success)
    {
        Success = success;
    }

    public CookieLoginResponse(CookieContainer cookies, string authenticityToken) : this(true)
    {
        Cookies = cookies;
        AuthenticityToken = authenticityToken;
    }


    [MemberNotNullWhen(true, nameof(Cookies))]
    [MemberNotNullWhen(true, nameof(AuthenticityToken))]
    public bool Success { get; private init; }
    public CookieContainer? Cookies { get; private init; }
    public string? AuthenticityToken { get; private init; }
}
