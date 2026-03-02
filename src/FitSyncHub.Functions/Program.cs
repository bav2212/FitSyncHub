using System.Text.Json;
using FitSyncHub.Common;
using FitSyncHub.Functions.Functions;
using FitSyncHub.Functions.Functions.IntervalsIcu;
using FitSyncHub.Functions.Functions.Zwift;
using FitSyncHub.Functions.Middlewares;
using FitSyncHub.Functions.Repositories;
using FitSyncHub.Functions.Services;
using FitSyncHub.GarminConnect;
using FitSyncHub.IntervalsICU;
using FitSyncHub.Strava;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Xert;
using FitSyncHub.Youtube;
using FitSyncHub.Zwift;
using Microsoft.AspNetCore.Http;
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
    nameof(MacroNutrientsCalculatorHttpTriggerFunction),
    nameof(LactateSyncHttpTriggerFunction),
    nameof(XertWorkoutToIntervalsICUExporterHttpTriggerFunction),
    nameof(ZwiftEventRidersCompetitionMetricsHttpTriggerFunction),
    nameof(ZwiftEventVELORatingHttpTriggerFunction),
    nameof(YoutubeRedirectToLiveChatHttpTriggerFunction),
];

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// apply ExceptionHandlingMiddleware for functions called by user only, otherwise will see error in azure portal
builder.UseWhen<ExceptionHandlingMiddleware>((context) => functionsCalledByUser.Contains(context.FunctionDefinition.Name));
builder.UseMiddleware<HttpContextAccessorMiddleware>();
builder.UseMiddleware<LogBadRequestMiddleware>();

builder.Configuration.AddUserSecrets<Program>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services
    .AddSingleton(_ => new CosmosClient(builder.Configuration["AzureWebJobsStorageConnectionString"], new()
    {
        UseSystemTextJsonSerializerWithOptions = new JsonSerializerOptions()
    }));

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


builder.Services.ConfigureCommonModule();
builder.Services.ConfigureStravaModule(builder.Configuration.GetSection("Strava"))
    .AddTransient<IStravaOAuthService, StravaOAuthService>();

builder.Services.ConfigureIntervalsIcuModule(builder.Configuration.GetSection("IntervalsICU"));
builder.Services.ConfigureGarminConnectModule(builder.Configuration.GetSection("GarminConnect:Credentials"));
builder.Services.ConfigureZwiftModule(builder.Configuration.GetSection("Zwift:Credentials"));
builder.Services.ConfigureYoutubeModule(builder.Configuration.GetSection("Youtube"));
builder.Services.ConfigureXertModule(builder.Configuration.GetSection("Xert"));


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
