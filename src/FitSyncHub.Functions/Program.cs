using System.Text.Json;
using Azure.Monitor.OpenTelemetry.Exporter;
using FitSyncHub.Common;
using FitSyncHub.Functions;
using FitSyncHub.Functions.Functions;
using FitSyncHub.Functions.Functions.IntervalsIcu;
using FitSyncHub.Functions.Functions.Zwift;
using FitSyncHub.Functions.Middlewares;
using FitSyncHub.Functions.Repositories;
using FitSyncHub.Functions.Services;
using FitSyncHub.GarminConnect;
using FitSyncHub.IntervalsICU;
using FitSyncHub.Strava;
using FitSyncHub.Xert;
using FitSyncHub.Youtube;
using FitSyncHub.Zwift;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

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
    nameof(ZwiftEventsToCompleteRouteAchievementsHttpTriggerFunction),
    nameof(ZwiftEventsWithPreselectedBikeHttpTriggerFunction),
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

if (builder.Environment.IsDevelopment())
{
    // keep simple logs
    builder.Logging.AddConsole();
}

builder.Services
    .AddOpenTelemetry()
    .UseFunctionsWorkerDefaults()
    .WithTracing(tracing =>
    {
        tracing.AddHttpClientInstrumentation();    // Trace outgoing HTTP calls
    })
    .WithMetrics(metrics =>
    {
        metrics.AddHttpClientInstrumentation();     //
    })
    .UseAzureMonitorExporter();

builder.Services
    .AddSingleton(_ => new CosmosClient(builder.Configuration[FitSyncHub.Functions.Constants.CosmosDb.ConnectionString], new()
    {
        UseSystemTextJsonSerializerWithOptions = new JsonSerializerOptions()
    }));

builder.Services.AddHttpContextAccessor();

builder.Services.AddCommonModule();
builder.Services.AddStravaModule(builder.Configuration.GetSection("Strava"))
    .AddTokenStore<CosmosDbStravaOAuthTokenStore>();

builder.Services.AddIntervalsIcuModule(builder.Configuration.GetSection("IntervalsICU"));
builder.Services.AddGarminConnectModule(builder.Configuration.GetSection("GarminConnect:Credentials"));
builder.Services.AddZwiftModule(builder.Configuration.GetSection("Zwift:Credentials"));
builder.Services.AddYoutubeModule(builder.Configuration.GetSection("Youtube"));
builder.Services.AddXertModule(builder.Configuration.GetSection("Xert"));

builder.Services.AddCosmosCache(cacheOptions =>
{
    cacheOptions.DatabaseName = FitSyncHub.Functions.Constants.CosmosDb.DatabaseName;
    cacheOptions.ContainerName = FitSyncHub.Functions.Constants.CosmosDb.Containers.DistributedCache;
    cacheOptions.ClientBuilder = new CosmosClientBuilder(builder.Configuration[FitSyncHub.Functions.Constants.CosmosDb.ConnectionString]);
    cacheOptions.CreateIfNotExists = true;
});

builder.Services.AddTransient<StravaStravaOAuthDataRepository>();
builder.Services.AddTransient<StravaSummaryActivityRepository>();

builder.Services.AddTransient<GarminHealthDataService>();
builder.Services.AddTransient<StravaSummaryActivityService>();
builder.Services.AddTransient<StravaUpdateActivityService>();

var host = builder.Build();
await host.RunAsync();
