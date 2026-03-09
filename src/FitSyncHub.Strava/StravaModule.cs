using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.DelegatingHandlers;
using FitSyncHub.Strava.HttpClients;
using FitSyncHub.Strava.Models.Responses;
using FitSyncHub.Strava.Options;
using FitSyncHub.Strava.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace FitSyncHub.Strava;

public static class StravaModule
{
    extension(IServiceCollection services)
    {
        public StravaModuleBuilder AddStravaModule(IConfigurationSection configurationSection)
        {
            return services.AddStravaModule(options => configurationSection.Bind(options));
        }

        public StravaModuleBuilder AddStravaModule(Action<StravaOptions> options)
        {
            services
                .AddOptions<StravaOptions>()
                .Configure(options)
                .Validate(_ => services.Any(s => s.ServiceType == typeof(IStravaOAuthTokenStore)))
                .ValidateOnStart();

            var builder = new StravaModuleBuilder(services);

            services
                .AddHttpClient<IStravaOAuthHttpClient, StravaOAuthHttpClient>((sp, client) =>
                {
                    var stravaOptions = sp.GetRequiredService<IOptions<StravaOptions>>().Value;
                    client.BaseAddress = new Uri(stravaOptions.BaseAddress);
                });

            AddStravaUploadActivityResiliencePipeline(services);

            services.AddTransient<StravaOAuthTokenService>();
            services.AddTransient<StravaAuthenticationDelegatingHandler>();
            services
                .AddHttpClient<IStravaHttpClient, StravaHttpClient>((sp, client) =>
                {
                    var stravaOptions = sp.GetRequiredService<IOptions<StravaOptions>>().Value;
                    client.BaseAddress = new Uri(stravaOptions.ApiAddress);
                })
                .AddHttpMessageHandler<StravaAuthenticationDelegatingHandler>();

            return builder;
        }
    }

    private static void AddStravaUploadActivityResiliencePipeline(IServiceCollection services)
    {
        services.AddResiliencePipeline<string, UploadActivityResponse>(
            Constants.ResiliencePipeline.StravaUploadActivityResiliencePipeline,
            (builder, context) =>
        {
            builder
                .AddRetry(new RetryStrategyOptions<UploadActivityResponse>
                {
                    ShouldHandle = new PredicateBuilder<UploadActivityResponse>()
                    .HandleResult(r => r.Status == "Your activity is still being processed."),
                    DelayGenerator = args =>
                    {
                        const int InitialWait = 4000;
                        var delay = TimeSpan.FromMilliseconds(InitialWait * Math.Pow(2, args.AttemptNumber - 1));
                        return ValueTask.FromResult<TimeSpan?>(delay);
                    },
                    MaxRetryAttempts = 3,
                    OnRetry = args =>
                    {
                        var loggerFactory = context.ServiceProvider.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("StravaUploadActivityResiliencePipeline");

                        var uploadResponse = args.Outcome.Result!;
                        logger.LogInformation("Upload status: {Status}, Error: {Error}, retry {RetryCount} after {Timespan}",
                                                uploadResponse.Status,
                                                uploadResponse.Error,
                                                args.AttemptNumber,
                                                args.RetryDelay);
                        return ValueTask.CompletedTask;
                    }
                });
        });
    }
}
