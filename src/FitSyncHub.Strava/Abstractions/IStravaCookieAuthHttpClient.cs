using System.Net;
using FitSyncHub.Strava.Models.Responses;

namespace FitSyncHub.Strava.Abstractions;

public interface IStravaCookieAuthHttpClient
{
    Task<CookieLoginResponse> Login(string username, string password, CancellationToken cancellationToken);
    Task<bool> CheckCookiesCorrect(CookieContainer cookies, CancellationToken cancellationToken);
}
