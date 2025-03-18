using FitSyncHub.Zwift.HttpClients;
using Microsoft.Extensions.DependencyInjection;

namespace FitSyncHub.Zwift;

public static class ZwiftModule
{
    public static IServiceCollection ConfigureZwiftModule(this IServiceCollection services, string bearerToken)
    {
        services.AddHttpClient<ZwiftDownloaderHttpClient>(client =>
        {
            client.DefaultRequestHeaders.Add("authorization", bearerToken);
            client.DefaultRequestHeaders.Add("accept", "application/json, text/plain, */*");
            client.DefaultRequestHeaders.Add("accept-language", "en,uk;q=0.9,en-US;q=0.8,ru;q=0.7,pt;q=0.6");
            client.DefaultRequestHeaders.Add("origin", "https://www.zwift.com");
            client.DefaultRequestHeaders.Add("priority", "u=1, i");
            client.DefaultRequestHeaders.Add("referer", "https://www.zwift.com/");
            client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not(A:Brand\";v=\"99\", \"Microsoft Edge\";v=\"133\", \"Chromium\";v=\"133\"");
            client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
            client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            client.DefaultRequestHeaders.Add("sec-fetch-site", "same-site");
            client.DefaultRequestHeaders.Add("source", "zwift-web");
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Safari/537.36 Edg/133.0.0.0");

            client.BaseAddress = new Uri("https://us-or-rly101.zwift.com");
        });

        return services;
    }
}
