using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StravaWebhooksAzureFunctions.Options;

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
    })
    .Build();

host.Run();
