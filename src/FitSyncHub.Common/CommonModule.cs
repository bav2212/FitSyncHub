using FitSyncHub.Common.Abstractions;
using FitSyncHub.Common.Fit;
using FitSyncHub.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FitSyncHub.Common;

public static class CommonModule
{
    public static IServiceCollection ConfigureCommonModule<
        TStravaApplicationOptionsProvider>(this IServiceCollection services)
        where TStravaApplicationOptionsProvider : class, IStravaApplicationOptionsProvider
    {
        services.AddScoped<IStravaApplicationOptionsProvider, TStravaApplicationOptionsProvider>();

        services.AddScoped<FitFileDecoder>();
        services.AddScoped<FitFileEncoder>();

        services.AddSingleton<IDistributedCacheService, DistributedCacheService>();

        return services;
    }
}
