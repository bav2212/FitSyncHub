using Garmin.Connect;
using Garmin.Connect.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace FitSyncHub.GarminConnect;

public static class GarminConnectModule
{
    public static IServiceCollection ConfigureGarminConnectModule(
        this IServiceCollection services,
        BasicAuthParameters basicAuthParameters)
    {
        services.AddSingleton(sp =>
            new GarminConnectClient(sp.GetRequiredService<GarminConnectContext>()));

        services.AddSingleton(sp =>
        {
            return new GarminConnectContext(new HttpClient(), basicAuthParameters);
        });

        return services;
    }
}
