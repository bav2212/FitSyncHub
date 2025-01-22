using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.DelegatingHandlers;
using FitSyncHub.Strava.HttpClients;
using Microsoft.Extensions.DependencyInjection;

namespace FitSyncHub.Strava;

public static class StravaModule
{
    public static IServiceCollection ConfigureStravaModule<
        TStravaAuthCookieStorageManager,
        TStravaOAuthService>(this IServiceCollection services)
        where TStravaAuthCookieStorageManager : class, IStravaAuthCookieStorageManager
        where TStravaOAuthService : class, IStravaOAuthService
    {
        services.AddScoped<StravaAthleteContext>();

        services.AddScoped<IStravaAuthCookieStorageManager, TStravaAuthCookieStorageManager>();
        services.AddTransient<IStravaOAuthService, TStravaOAuthService>();

        services.AddTransient<IStravaCookieHttpClient, StravaCookieHttpClient>();
        services.AddScoped<IStravaCookieAuthHttpClient, StravaCookieAuthHttpClient>();
        services.Decorate<IStravaCookieAuthHttpClient, StravaCookieAuthHttpClientCached>();

        services.AddHttpClient<IStravaOAuthHttpClient, StravaOAuthHttpClient>(client =>
        {
            client.BaseAddress = new Uri("http://www.strava.com");
        });

        services.AddTransient<StravaRestApiAuthenticationDelegatingHandler>();
        services.AddHttpClient<IStravaRestHttpClient, StravaRestHttpClient>(client =>
        {
            client.BaseAddress = new Uri("https://www.strava.com/api/v3/");
        })
        .AddHttpMessageHandler<StravaRestApiAuthenticationDelegatingHandler>();

        return services;
    }
}
