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
        public IServiceCollection ConfigureGarminConnectModule(string garminAuthOptionsPath)
        {
            services.AddScoped<GarminConnectToIntervalsIcuRideWorkoutStepConverterInitializer>();
            services.AddScoped<GarminConnectToIntervalsIcuStrengthWorkoutStepConverterInitializer>();
            services.AddScoped(ConverterInitializerImplementationFactory);
            services.AddScoped<GarminConnectToInternalWorkoutConverterService>();

            ConfigureAuth(services, garminAuthOptionsPath);

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

    private static void ConfigureAuth(IServiceCollection services, string garminAuthOptionsPath)
    {
        services.AddOptions<GarminConnectAuthOptions>()
          .Configure<IConfiguration>((settings, configuration) => configuration.GetSection(garminAuthOptionsPath).Bind(settings))
          .ValidateOnStart();

        services.AddScoped<GarminSsoHttpClient>();
        services.AddScoped<GarminSsoCookieContainer>(); // Scoped if per user/session, or Singleton if shared app-wide

        services.AddHttpClient("GarminSSOClient", client =>
            {
                client.BaseAddress = new Uri("https://sso.garmin.com/sso");

                client.DefaultRequestHeaders.Add("User-Agent", "GCM-iOS-5.7.2.1");
                client.DefaultRequestHeaders.Add("origin", "https://sso.garmin.com");
            })
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                return new HttpClientHandler
                {
                    CookieContainer = sp.GetRequiredService<GarminSsoCookieContainer>(),
                    UseCookies = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
            });

        services.AddHttpClient<GarminOAuthHttpClient>(client =>
        {
            client.BaseAddress = new Uri("https://connectapi.garmin.com/oauth-service/oauth");

            client.DefaultRequestHeaders.Add("User-Agent", "com.garmin.android.apps.connectmobile");
        });

        services.AddHttpClient<IGarminConsumerCredentialsProvider, GarminConsumerCredentialsHttpClient>(
            client => client.BaseAddress = new Uri("https://thegarth.s3.amazonaws.com"));
        services.Decorate<IGarminConsumerCredentialsProvider, GarminConsumerCredentialsProviderCached>();

        services.AddScoped<IGarminAuthService, GarminAuthService>();
        services.AddScoped<IGarminTokenExchanger, GarminAuthService>();
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
