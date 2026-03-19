using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.Options;
using FitSyncHub.IntervalsICU.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FitSyncHub.IntervalsICU;

public static class IntervalsIcuModule
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddIntervalsIcuModule(IConfigurationSection configurationSection)
        {
            return services.AddIntervalsIcuModule(options => configurationSection.Bind(options));
        }

        public IServiceCollection AddIntervalsIcuModule(Action<IntervalsIcuOptions> options)
        {
            services.Configure(options);

            services.AddScoped<IntervalsIcuStorageService>();
            services.AddScoped<IntervalsIcuDeletePlanService>();
            services.AddScoped<WhatsOnZwiftToIntervalsIcuService>();
            services.AddScoped<WhatsOnZwiftScraperService>();
            services.AddHttpClient<IntervalsIcuHttpClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<IntervalsIcuOptions>>().Value;
                var intervalsIcuApiKey = options.ApiKey ?? throw new InvalidOperationException("IntervalsIcuApiKey is null");
                client.BaseAddress = new Uri(options.ApiAddress);

                var authenticationString = $"API_KEY:{intervalsIcuApiKey}";
                var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            }).ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            });

            return services;
        }
    }
}
