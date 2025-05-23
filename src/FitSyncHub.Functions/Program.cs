using FitSyncHub.Common;
using FitSyncHub.Functions;
using FitSyncHub.Functions.Functions;
using FitSyncHub.Functions.Functions.IntervalsIcu;
using FitSyncHub.Functions.Options;
using FitSyncHub.Functions.Repositories;
using FitSyncHub.Functions.Services;
using FitSyncHub.GarminConnect;
using FitSyncHub.IntervalsICU;
using FitSyncHub.Strava;
using FitSyncHub.Zwift;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HashSet<string> functionsCalledByUser = [
    nameof(WeightHttpTriggerFunction),
    nameof(SyncIntervalsICUWithGarminHttpTriggerFunction),
    nameof(WhatsOnZwiftToIntervalsICUConverterHttpTriggerFunction),
    nameof(GarminWorkoutToIntervalsICUExporterHttpTriggerFunction),
    nameof(GarminWorkoutUploadToStravaHttpTriggerFunction),
];

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// apply ExceptionHandlingMiddleware for functions called by user only, otherwise will see error in azure portal
builder.UseWhen<ExceptionHandlingMiddleware>((context) => functionsCalledByUser.Contains(context.FunctionDefinition.Name));

builder.Configuration.AddUserSecrets<Program>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddSingleton(_ => new CosmosClient(builder.Configuration["AzureWebJobsStorageConnectionString"]));

builder.Services.AddOptions<BodyMeasurementsOptions>()
    .Configure<IConfiguration>((settings, configuration) => configuration.GetSection(BodyMeasurementsOptions.Position).Bind(settings));

builder.Services.ConfigureCommonModule<StravaApplicationOptionsProvider>();
builder.Services.ConfigureStravaModule<StravaOAuthService>();
builder.Services.ConfigureIntervalsIcuModule();
builder.Services.ConfigureGarminConnectModule("GarminConnect:Credentials");
builder.Services.ConfigureZwiftModule("Zwift:Credentials");
//builder.Services.ConfigureZwiftInsiderModule();

builder.Services.AddCosmosCache(cacheOptions =>
{
    cacheOptions.DatabaseName = "fit-sync-hub";
    cacheOptions.ContainerName = "DistributedCache";
    cacheOptions.ClientBuilder = new CosmosClientBuilder(builder.Configuration["AzureWebJobsStorageConnectionString"]);
    cacheOptions.CreateIfNotExists = true;
});

builder.Services.AddTransient<PersistedGrantRepository>();
builder.Services.AddTransient<SummaryActivityRepository>();

builder.Services.AddTransient<GarminHealthDataService>();
builder.Services.AddTransient<StravaSummaryActivityService>();
builder.Services.AddTransient<StravaUpdateActivityService>();

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
