using System.Net;
using FitSyncHub.GarminConnect.Auth;
using FitSyncHub.GarminConnect.Auth.Abstractions;
using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.GarminConnect.Options;
using FitSyncHub.GarminConnect.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitSyncHub.GarminConnect;

public static class GarminConnectModule
{
    public static IServiceCollection ConfigureGarminConnectModule(
        this IServiceCollection services,
        string garminAuthOptionsPath)
    {
        services.AddScoped<GarminConnectToIntervalsIcuWorkoutStepConverterInitializer>();

        services.AddOptions<GarminConnectAuthOptions>()
            .Configure<IConfiguration>((settings, configuration) => configuration.GetSection(garminAuthOptionsPath).Bind(settings))
            .ValidateOnStart();

        services.AddHttpClient<IGarminAuthenticationService, GarminAuthenticationService>();
        services.AddTransient<GarminConnectAuthenticationDelegatingHandler>();
        services.AddHttpClient<GarminConnectHttpClient>(client => client.BaseAddress = new Uri("https://connect.garmin.com"))
            .AddHttpMessageHandler<GarminConnectAuthenticationDelegatingHandler>()
            .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            });

        return services;
    }
}
