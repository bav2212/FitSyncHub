using System.Net.Http.Headers;
using System.Text;
using FitSyncHub.Functions.HttpClients;
using FitSyncHub.Functions.HttpClients.Interfaces;
using FitSyncHub.Functions.Options;
using FitSyncHub.Functions.Repositories;
using FitSyncHub.Functions.Services;
using FitSyncHub.Functions.Services.Interfaces;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((hostBuilderContext, services) =>
    {
        services.AddSingleton(x => new CosmosClient(hostBuilderContext.Configuration["AzureWebJobsStorageConnectionString"]));

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.Configure<LoggerFilterOptions>(options =>
        {
            // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
            // Log levels can also be configured using host.json. For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
            var toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

            if (toRemove is not null)
            {
                options.Rules.Remove(toRemove);
            }
        });

        services.AddOptions<StravaOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(StravaOptions.Position).Bind(settings);
            });

        services.AddOptions<BodyMeasurementsOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(BodyMeasurementsOptions.Position).Bind(settings);
            });

        services.AddScoped<IStravaCookieAuthHttpClient, StravaCookieAuthHttpClient>();
        services.Decorate<IStravaCookieAuthHttpClient, StravaCookieAuthHttpClientCached>();

        services.AddHttpClient<IStravaOAuthHttpClient, StravaOAuthHttpClient>(client =>
        {
            client.BaseAddress = new Uri("http://www.strava.com");
        });

        services.AddHttpClient<IStravaRestHttpClient, StravaRestHttpClient>(client =>
        {
            client.BaseAddress = new Uri("https://www.strava.com/api/v3/");
        });
        services.AddTransient<IStravaCookieHttpClient, StravaCookieHttpClient>();

        services.AddTransient<PersistedGrantRepository>();
        services.AddTransient<SummaryActivityRepository>();
        services.AddTransient<UserSessionRepository>();

        services.AddTransient<CorrectElevationService>();
        services.AddTransient<StoreSummaryActivitiesService>();
        services.AddTransient<UpdateActivityService>();
        services.AddTransient<IStravaOAuthService, StravaOAuthService>();

        ///
        services.AddScoped<ZwiftToIntervalsIcuService>();
        services.AddScoped<IntervalsIcuStorageService>();

        services.AddHttpClient<IntervalsIcuHttpClient>(client =>
        {
            var intervalsIcuApiKey = hostBuilderContext.Configuration["IntervalsIcuApiKey"];

            client.BaseAddress = new Uri("https://intervals.icu");

            var authenticationString = $"API_KEY:{intervalsIcuApiKey}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
        });

    })
    .Build();

host.Run();
