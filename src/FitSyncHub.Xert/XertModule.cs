using FitSyncHub.Xert.Abstractions;
using FitSyncHub.Xert.DelegatingHandlers;
using FitSyncHub.Xert.HttpClients;
using FitSyncHub.Xert.Options;
using FitSyncHub.Xert.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitSyncHub.Xert;

public static class XertModule
{
    public static IServiceCollection ConfigureXertModule(this IServiceCollection services, string xertOptionsPath)
    {
        services.AddOptions<XertOptions>()
            .Configure<IConfiguration>((settings, configuration) => configuration.GetSection(xertOptionsPath).Bind(settings))
            .ValidateOnStart();

        services.AddHttpClient<IXertAuthHttpClient, XertAuthHttpClient>(client
            => client.BaseAddress = new Uri("https://www.xertonline.com"));

        services.AddTransient<XertAuthenticationDelegatingHandler>();
        services.AddHttpClient<IXertHttpClient, XertHttpClient>(client
            => client.BaseAddress = new Uri("https://www.xertonline.com"))
            .AddHttpMessageHandler<XertAuthenticationDelegatingHandler>();

        services.AddScoped<IXertAuthService, XertAuthService>();

        return services;
    }
}
