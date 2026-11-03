using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Mochi;
using FitSyncHub.Zwift.JsonSerializerContexts;

namespace FitSyncHub.Zwift.HttpClients;

public class ZwiftMochiHttpClient
{
    private readonly HttpClient _httpClient;

    public ZwiftMochiHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<MochiCatalogResponse> GetMochiCatalogResponse(CancellationToken cancellationToken)
    {
        var mochiCatalogResponseMessage = await _httpClient.GetAsync("watopian/watopian.manifest.json", cancellationToken);
        mochiCatalogResponseMessage.EnsureSuccessStatusCode();

        var mochiCatalogResponseContent = await mochiCatalogResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

        return JsonSerializer.Deserialize(mochiCatalogResponseContent, MochiCatalogGenerationContext.Default.MochiCatalogResponse)!;
    }

    public async Task DownloadMochiItem(MochiAssetResponse route, string destination, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(route.Key, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);

        await using var target = new FileStream(
                    destination,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true);

        await source.CopyToAsync(target, cancellationToken);
    }
}
