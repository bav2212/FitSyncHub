using FitSyncHub.Common.Abstractions;
using FitSyncHub.Weather.HttpClients;
using FitSyncHub.Weather.Options;
using FitSyncHub.Weather.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Weather;

public static class WeatherModule
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWeatherModule(IConfigurationSection configurationSection)
        {
            return services.AddWeatherModule(options => configurationSection.Bind(options));
        }

        public IServiceCollection AddWeatherModule(Action<WeatherOptions> options)
        {
            services.Configure(options);

            services.AddHttpClient<IOpenMeteoHttpClient, OpenMeteoHttpClient>();
            services.Decorate<IOpenMeteoHttpClient, CachedOpenMeteoHttpClient>();

            services.AddScoped<IWeatherService, OpenMeteoWeatherService>();

            return services;
        }
    }
}
