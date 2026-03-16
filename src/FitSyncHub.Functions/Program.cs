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
using OpenTelemetry.Logs;

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

builder.Logging.AddOpenTelemetry(logging =>
{
    if (builder.Environment.IsDevelopment())
    {
        logging.AddConsoleExporter();
    }
});

builder.Services
    .AddOpenTelemetry()
    .UseFunctionsWorkerDefaults()
    .UseAzureMonitorExporter();

builder.Services
    .AddSingleton(_ => new CosmosClient(builder.Configuration["AzureWebJobsStorageConnectionString"], new()
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
    cacheOptions.DatabaseName = "fit-sync-hub";
    cacheOptions.ContainerName = "DistributedCache";
    cacheOptions.ClientBuilder = new CosmosClientBuilder(builder.Configuration["AzureWebJobsStorageConnectionString"]);
    cacheOptions.CreateIfNotExists = true;
});

builder.Services.AddTransient<StravaStravaOAuthDataRepository>();
builder.Services.AddTransient<StravaSummaryActivityRepository>();

builder.Services.AddTransient<GarminHealthDataService>();
builder.Services.AddTransient<StravaSummaryActivityService>();
builder.Services.AddTransient<StravaUpdateActivityService>();

var host = builder.Build();
await host.RunAsync();
