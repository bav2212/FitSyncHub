using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StravaWebhooksAzureFunctions.HttpClients;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.Options;
using StravaWebhooksAzureFunctions.Services;
using StravaWebhooksAzureFunctions.Services.Interfaces;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddOptions<StravaOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(StravaOptions.Position).Bind(settings);
            });

        // try to use Scruptor to decorate
        services.AddScoped<StravaCookieAuthHttpClient>();
        services.AddScoped<IStravaCookieAuthHttpClient, StravaCookieAuthHttpClientCached>();

        services.AddHttpClient<IStravaOAuthHttpClient, StravaOAuthHttpClient>(client =>
        {
            client.BaseAddress = new Uri("http://www.strava.com");
        });

        services.AddHttpClient<IStravaRestHttpClient, StravaRestHttpClient>(client =>
        {
            client.BaseAddress = new Uri("https://www.strava.com/api/v3/");
        });
        services.AddTransient<IStravaCookieHttpClient, StravaCookieHttpClient>();

        services.AddTransient<RestApiAuthTokenService>();
        services.AddTransient<UpdateActivityService>();
        services.AddTransient<IStravaOAuthService, StravaOAuthService>();
    })
    .Build();

host.Run();
