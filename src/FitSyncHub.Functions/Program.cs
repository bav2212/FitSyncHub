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
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

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

builder.Services.AddScoped<IStravaCookieAuthHttpClient, StravaCookieAuthHttpClient>();
builder.Services.Decorate<IStravaCookieAuthHttpClient, StravaCookieAuthHttpClientCached>();

builder.Services.AddHttpClient<IStravaOAuthHttpClient, StravaOAuthHttpClient>(client =>
{
    client.BaseAddress = new Uri("http://www.strava.com");
});

builder.Services.AddHttpClient<IStravaRestHttpClient, StravaRestHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://www.strava.com/api/v3/");
});
builder.Services.AddTransient<IStravaCookieHttpClient, StravaCookieHttpClient>();

builder.Services.AddTransient<PersistedGrantRepository>();
builder.Services.AddTransient<SummaryActivityRepository>();
builder.Services.AddTransient<UserSessionRepository>();

builder.Services.AddTransient<CorrectElevationService>();
builder.Services.AddTransient<StoreSummaryActivitiesService>();
builder.Services.AddTransient<UpdateActivityService>();
builder.Services.AddTransient<IStravaOAuthService, StravaOAuthService>();

///
builder.Services.AddScoped<ZwiftToIntervalsIcuService>();
builder.Services.AddScoped<IntervalsIcuStorageService>();
builder.Services.AddScoped<IntervalsIcuDeletePlanService>();

builder.Services.AddHttpClient<IntervalsIcuHttpClient>(client =>
{
    var intervalsIcuApiKey = builder.Configuration["IntervalsIcuApiKey"] ?? throw new InvalidOperationException("IntervalsIcuApiKey is null");

    client.BaseAddress = new Uri("https://intervals.icu");

    var authenticationString = $"API_KEY:{intervalsIcuApiKey}";
    var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
});

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
