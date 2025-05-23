using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.DelegatingHandlers;
using FitSyncHub.Strava.HttpClients;
using FitSyncHub.Strava.Models.Responses;
using FitSyncHub.Strava.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace FitSyncHub.Strava;

public static class StravaModule
{
    public static IServiceCollection ConfigureStravaModule<
        TStravaOAuthService>(this IServiceCollection services)
        where TStravaOAuthService : class, IStravaOAuthService
    {
        services.AddOptions<StravaOptions>().Configure<IConfiguration>((settings, configuration)
            => configuration.GetSection(StravaOptions.Position).Bind(settings));

        services.AddTransient<IStravaOAuthService, TStravaOAuthService>();
        services.AddHttpClient<IStravaOAuthHttpClient, StravaOAuthHttpClient>(client
            => client.BaseAddress = new Uri("http://www.strava.com"));

        AddStravaUploadActivityResiliencePipeline(services);

        services.AddTransient<StravaAuthenticationDelegatingHandler>();
        services.AddHttpClient<IStravaHttpClient, StravaHttpClient>(client
            => client.BaseAddress = new Uri("https://www.strava.com/api/v3/"))
        .AddHttpMessageHandler<StravaAuthenticationDelegatingHandler>();

        return services;
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
