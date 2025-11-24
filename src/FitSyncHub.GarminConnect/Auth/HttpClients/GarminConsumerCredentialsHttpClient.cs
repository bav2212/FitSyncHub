using System.Text.Json;
using FitSyncHub.GarminConnect.Auth.Abstractions;
using FitSyncHub.GarminConnect.Auth.Models;
using FitSyncHub.GarminConnect.JsonSerializerContexts;

namespace FitSyncHub.GarminConnect.Auth.HttpClients;

internal sealed class GarminConsumerCredentialsHttpClient : IGarminConsumerCredentialsProvider
{
    private readonly HttpClient _httpClient;

    public GarminConsumerCredentialsHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GarminConsumerCredentials> GetConsumerCredentials(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("oauth_consumer.json", cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content, GarminConnectOAuthSerializerContext.Default.GarminConsumerCredentials)
            ?? throw new InvalidOperationException("Failed to deserialize ConsumerCredentials from Garmin response.");
    }
}

internal sealed class GarminConsumerCredentialsProviderCached : IGarminConsumerCredentialsProvider
{
    private readonly IGarminConsumerCredentialsProvider _provider;
    private GarminConsumerCredentials? _consumerCredentials;

    public GarminConsumerCredentialsProviderCached(
        IGarminConsumerCredentialsProvider provider)
    {
        _provider = provider;
    }

    public async Task<GarminConsumerCredentials> GetConsumerCredentials(CancellationToken cancellationToken)
    {
        if (_consumerCredentials is not null)
        {
            return _consumerCredentials;
        }

        _consumerCredentials = await _provider.GetConsumerCredentials(cancellationToken);
        return _consumerCredentials;
    }
}
