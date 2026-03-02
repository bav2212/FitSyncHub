using FitSyncHub.Common.Fit;
using FitSyncHub.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FitSyncHub.Common;

public static class CommonModule
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureCommonModule()
        {
            services.AddScoped<FitFileDecoder>();
            services.AddScoped<FitFileEncoder>();

            services.AddSingleton<IDistributedCacheService, DistributedCacheService>();

            return services;
        }
    }
}
