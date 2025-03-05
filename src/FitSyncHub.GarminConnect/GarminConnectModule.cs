using Garmin.Connect.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace FitSyncHub.GarminConnect;

public static class GarminConnectModule
{
    public static IServiceCollection ConfigureGarminConnectModule(
        this IServiceCollection services,
        string? email,
        string? password)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(password));
        }

        services.AddMemoryCache();
        services.AddSingleton<IAuthParameters>((sp) => new BasicAuthParameters(email, password));

        services.AddHttpClient<IGarminAuthenticationService, GarminAuthenticationService>();
        services.AddTransient<GarminConnectAuthenticationDelegatingHandler>();
        services.AddHttpClient<GarminConnectHttpClient>((sp, client) =>
        {
            var baseAddress = sp.GetRequiredService<IAuthParameters>().BaseUrl;
            client.BaseAddress = new Uri(baseAddress);
        })
        .AddHttpMessageHandler<GarminConnectAuthenticationDelegatingHandler>();

        return services;
    }
}
