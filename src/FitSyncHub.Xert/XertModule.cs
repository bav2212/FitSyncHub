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
    extension(IServiceCollection services)
    {
        public IServiceCollection AddXertModule(IConfigurationSection configurationSection)
        {
            return services.AddXertModule(options => configurationSection.Bind(options));
        }

        public IServiceCollection AddXertModule(Action<XertOptions> options)
        {
            services.Configure(options);

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
}
