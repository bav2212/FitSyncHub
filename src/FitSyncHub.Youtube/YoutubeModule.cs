using FitSyncHub.Youtube.Options;
using FitSyncHub.Youtube.Services;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Youtube;

public static class YoutubeModule
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureYoutubeModule(string youtubeOptionsPath)
        {
            services.AddOptions<YoutubeOptions>()
                .Configure<IConfiguration>((settings, configuration) => configuration.GetSection(youtubeOptionsPath).Bind(settings))
                .ValidateOnStart();

            services.AddScoped(sp =>
            {
                var options = sp.GetRequiredService<IOptions<YoutubeOptions>>().Value;
                return new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = options.ApiKey,
                });
            });

            services.AddScoped<YouTubeLiveService>();

            return services;
        }
    }
}
