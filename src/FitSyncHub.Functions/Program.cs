using FitSyncHub.Common;
using FitSyncHub.Functions;
using FitSyncHub.Functions.Managers;
using FitSyncHub.Functions.Options;
using FitSyncHub.Functions.Repositories;
using FitSyncHub.Functions.Services;
using FitSyncHub.GarminConnect;
using FitSyncHub.IntervalsICU;
using FitSyncHub.Strava;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.UseMiddleware<ExceptionHandlingMiddleware>();

builder.Configuration.AddUserSecrets<Program>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddSingleton(x => new CosmosClient(builder.Configuration["AzureWebJobsStorageConnectionString"]));

builder.Services.AddOptions<StravaOptions>()
    .Configure<IConfiguration>((settings, configuration) =>
    {
        configuration.GetSection(StravaOptions.Position).Bind(settings);
    });

builder.Services.AddOptions<BodyMeasurementsOptions>()
    .Configure<IConfiguration>((settings, configuration) =>
    {
        configuration.GetSection(BodyMeasurementsOptions.Position).Bind(settings);
    });

builder.Services.ConfigureCommonModule<StravaApplicationOptionsProvider>();
builder.Services.ConfigureStravaModule<StravaAuthCookieStorageManager, StravaOAuthService>();
builder.Services.ConfigureIntervalsIcuModule(builder.Configuration["IntervalsIcuApiKey"]);
builder.Services.ConfigureGarminConnectModule(
    builder.Configuration["GarminConnect:Credentials:Username"],
    builder.Configuration["GarminConnect:Credentials:Password"]
);

//builder.Services.ConfigureZwiftInsiderModule();

builder.Services.AddTransient<PersistedGrantRepository>();
builder.Services.AddTransient<SummaryActivityRepository>();
builder.Services.AddTransient<UserSessionRepository>();

builder.Services.AddTransient<CorrectElevationService>();
builder.Services.AddTransient<SummaryActivityService>();
builder.Services.AddTransient<UpdateActivityService>();

builder.Logging.Services.Configure<LoggerFilterOptions>(options =>
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

var host = builder.Build();
await host.RunAsync();
