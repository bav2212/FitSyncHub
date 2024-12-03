using System.Net;
using FitSyncHub.Functions.HttpClients.Models.Responses;

namespace FitSyncHub.Functions.HttpClients.Interfaces;

public interface IStravaCookieAuthHttpClient
{
    Task<CookieLoginResponse> Login(string username, string password, CancellationToken cancellationToken);
    Task<bool> CheckCookiesCorrect(CookieContainer cookies, CancellationToken cancellationToken);
}
