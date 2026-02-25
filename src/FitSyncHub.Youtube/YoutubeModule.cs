using FitSyncHub.Youtube.Options;
using FitSyncHub.Youtube.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
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

                var token = new TokenResponse
                {
                    // Google Refresh token does NOT expire unless revoked
                    RefreshToken = options.RefreshToken
                };

                var initializer = new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = options.ClientId,
                        ClientSecret = options.ClientSecret,
                    },
                    Scopes = [YouTubeService.Scope.YoutubeReadonly],
                };

                var credential = new UserCredential(new GoogleAuthorizationCodeFlow(initializer), "user", token);

                // Create the service.
                return new YouTubeService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                });
            });

            services.AddScoped<YouTubeLiveService>();

            return services;
        }
    }
}
