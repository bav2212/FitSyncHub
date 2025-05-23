using FitSyncHub.ZwiftInsider.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FitSyncHub.ZwiftInsider;

public static class ZwiftInsiderModule
{
    public static IServiceCollection ConfigureZwiftInsiderModule(this IServiceCollection services)
    {
        services.AddScoped<ExcelReader>();
        services.AddScoped<ZwiftInsiderRoutesService>();
        services.AddScoped<ZwiftInsiderScraperService>();

        return services;
    }
}
