using FitSyncHub.Common.Abstractions;
using FitSyncHub.Common.Fit;
using FitSyncHub.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FitSyncHub.Common;

public static class CommonModule
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureCommonModule<TStravaApplicationOptionsProvider>()
            where TStravaApplicationOptionsProvider : class, IStravaApplicationOptionsProvider
        {
            services.AddScoped<IStravaApplicationOptionsProvider, TStravaApplicationOptionsProvider>();

            services.AddScoped<FitFileDecoder>();
            services.AddScoped<FitFileEncoder>();

            services.AddSingleton<IDistributedCacheService, DistributedCacheService>();

            return services;
        }
    }
}
