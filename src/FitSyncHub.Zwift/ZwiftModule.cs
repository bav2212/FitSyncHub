using System.Net;
using FitSyncHub.Zwift.Auth;
using FitSyncHub.Zwift.Auth.Abstractions;
using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.HttpClients.DelegatingHandlers;
using FitSyncHub.Zwift.Options;
using FitSyncHub.Zwift.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace FitSyncHub.Zwift;

public static class ZwiftModule
{
    public static IServiceCollection ConfigureZwiftModule(
        this IServiceCollection services,
        string zwiftAuthOptionsPath)
    {
        services.AddOptions<ZwiftAuthOptions>()
            .Configure<IConfiguration>((settings, configuration) => configuration
                .GetSection(zwiftAuthOptionsPath)
                .Bind(settings))
            .ValidateOnStart();

        services.AddScoped<ZwiftEventsService>();
        services.AddScoped<ZwiftPowerService>();
        services.AddScoped<ZwiftResultsAnalyzerService>();
        services.AddScoped<ZwiftGameInfoService>();

        services.AddHttpClient<IZwiftAuthenticator, ZwiftAuthHttpClient>(
            client => client.BaseAddress = new Uri("https://secure.zwift.com"));
        services.Decorate<IZwiftAuthenticator, ZwiftAuthHttpClientCached>();
        services.AddTransient<IZwiftTokenRefresher, ZwiftAuthHttpClient>();
        services.AddTransient<IZwiftAuthCacheInvalidator, ZwiftAuthHttpClientCached>();

        AddZwiftAuthResiliencePipeline(services);

        services.AddHttpClient<ZwiftHttpClientUnauthorized>();

        services.AddTransient<ZwiftAuthDelegatingHandler>();
        services.AddHttpClient<ZwiftHttpClient>(client => client.BaseAddress = new Uri("https://us-or-rly101.zwift.com"))
            .AddHttpMessageHandler<ZwiftAuthDelegatingHandler>()
            .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            });

        services.AddTransient<ZwiftRacingAuthDelegatingHandler>();
        services.AddHttpClient<ZwiftRacingHttpClient>(client => client.BaseAddress = new Uri("https://www.zwiftracing.app"))
            .AddHttpMessageHandler<ZwiftRacingAuthDelegatingHandler>();

        return services;
    }

    private static void AddZwiftAuthResiliencePipeline(IServiceCollection services)
    {
        const int RetryCount = 3;
        const int InitialWaitDurationMilliseconds = 200;

        services.AddResiliencePipeline<string, HttpResponseMessage>(Constants.ZwiftAuthResiliencePipeline, (builder, context) =>
        {
            // Resolve any service from DI
            var authenticationCache = context.ServiceProvider.GetRequiredService<IZwiftAuthCacheInvalidator>();

            builder
                .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .HandleResult(r => r.StatusCode == HttpStatusCode.Unauthorized),
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromMilliseconds(InitialWaitDurationMilliseconds),
                    MaxRetryAttempts = RetryCount,
                    OnRetry = async args =>
                    {
                        // Remove the cached authentication result on 401 Unauthorized
                        await authenticationCache.Invalidate(args.Context.CancellationToken);

                        var loggerFactory = context.ServiceProvider.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("ZwiftAuthResiliencePipeline");

                        logger.LogWarning("Received 401 Unauthorized, retry {RetryCount} after {Timespan}",
                            args.AttemptNumber, args.RetryDelay);
                    }
                });
        });
    }
}
