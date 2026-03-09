using FitSyncHub.Strava.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FitSyncHub.Strava;

public class StravaModuleBuilder
{
    public IServiceCollection Services { get; }

    internal StravaModuleBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public StravaModuleBuilder AddTokenStore<T>()
        where T : class, IStravaOAuthTokenStore
    {
        Services.AddScoped<IStravaOAuthTokenStore, T>();
        return this;
    }
}
