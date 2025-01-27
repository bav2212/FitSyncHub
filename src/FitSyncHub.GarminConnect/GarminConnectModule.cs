using Garmin.Connect;
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

        services.AddSingleton<IAuthParameters>((sp) => new BasicAuthParameters(email, password));
        services.AddHttpClient<GarminConnectContext>();

        services.AddScoped(sp => new GarminConnectClient(sp.GetRequiredService<GarminConnectContext>()));

        return services;
    }
}
