using System.Net;
using FitSyncHub.Common.Workouts;
using FitSyncHub.GarminConnect.Auth;
using FitSyncHub.GarminConnect.Auth.Abstractions;
using FitSyncHub.GarminConnect.Auth.HttpClients;
using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.GarminConnect.Options;
using FitSyncHub.GarminConnect.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitSyncHub.GarminConnect;

public static class GarminConnectModule
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddGarminConnectModule(IConfigurationSection configurationSection)
        {
            return services.AddGarminConnectModule(options => configurationSection.Bind(options));
        }

        public IServiceCollection AddGarminConnectModule(Action<GarminConnectAuthOptions> options)
        {
            services.AddScoped<GarminConnectToIntervalsIcuRideWorkoutStepConverterInitializer>();
            services.AddScoped<GarminConnectToIntervalsIcuStrengthWorkoutStepConverterInitializer>();
            services.AddScoped(ConverterInitializerImplementationFactory);
            services.AddScoped<GarminConnectToInternalWorkoutConverterService>();

            ConfigureAuth(services, options);

            services.AddTransient<GarminConnectAuthenticationDelegatingHandler>();
            services.AddHttpClient<GarminConnectHttpClient>(client =>
                    client.BaseAddress = new Uri("https://connectapi.garmin.com")
                )
                .AddHttpMessageHandler<GarminConnectAuthenticationDelegatingHandler>()
                .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
                });

            return services;
        }
    }

    private static void ConfigureAuth(IServiceCollection services, Action<GarminConnectAuthOptions> options)
    {
        services.Configure(options);

        services.AddHttpClient<GarminSsoHttpClient>();
        services.AddHttpClient<GarminDiHttpClient>();

        services.AddScoped<IGarminAuthService, GarminAuthService>();
        services.AddScoped<IGarminTokenRefresher, GarminAuthService>();
        services.AddScoped<IGarminAuthProvider, GarminAuthService>();
        services.AddScoped<IGarminAuthCacheInvalidator, GarminAuthService>();
    }

    private static Func<WorkoutType, IGarminConnectToIntervalsIcuWorkoutStepConverterInitializer> ConverterInitializerImplementationFactory(IServiceProvider sp)
        => stepKey => stepKey switch
        {
            WorkoutType.Ride => sp.GetRequiredService<GarminConnectToIntervalsIcuRideWorkoutStepConverterInitializer>(),
            WorkoutType.Strength => sp.GetRequiredService<GarminConnectToIntervalsIcuStrengthWorkoutStepConverterInitializer>(),
            _ => throw new NotImplementedException($"No converter found for step key: {stepKey}")
        };
}
