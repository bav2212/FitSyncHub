using FitSyncHub.Zwift.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FitSyncHub.Zwift;

public static class ZwiftModule
{
    public static IServiceCollection ConfigureZwiftModule(this IServiceCollection services)
    {
        services.AddScoped<ExcelReader>();
        services.AddScoped<ZwiftRoutesService>();

        return services;
    }
}
