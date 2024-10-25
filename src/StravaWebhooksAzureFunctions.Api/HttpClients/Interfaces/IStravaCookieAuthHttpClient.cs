using System.Net;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses;

namespace StravaWebhooksAzureFunctions.HttpClients.Interfaces;

public interface IStravaCookieAuthHttpClient
{
    Task<CookieLoginResponse> Login(string username, string password, CancellationToken cancellationToken);
    Task<bool> CheckCookiesCorrect(CookieContainer cookies, string authenticityToken, CancellationToken cancellationToken);
}
