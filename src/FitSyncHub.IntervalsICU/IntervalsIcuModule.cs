using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FitSyncHub.IntervalsICU;

public static class IntervalsIcuModule
{
    public static IServiceCollection ConfigureIntervalsIcuModule(this IServiceCollection services,
        string? intervalsIcuApiKey)
    {
        services.AddScoped<ZwiftToIntervalsIcuService>();
        services.AddScoped<IntervalsIcuStorageService>();
        services.AddScoped<IntervalsIcuDeletePlanService>();
        services.AddHttpClient<IntervalsIcuHttpClient>(client =>
        {
            if (intervalsIcuApiKey is null)
            {
                throw new InvalidOperationException("IntervalsIcuApiKey is null");
            }
            client.BaseAddress = new Uri("https://intervals.icu");

            var authenticationString = $"API_KEY:{intervalsIcuApiKey}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
        }).ConfigurePrimaryHttpMessageHandler(config => new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
        });

        return services;
    }
}
