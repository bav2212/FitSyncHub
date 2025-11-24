using FitSyncHub.Zwift.HttpClients.Models.Requests.Activities;

namespace FitSyncHub.Zwift.HttpClients;

public sealed class ZwiftHttpClientUnauthorized
{
    private readonly HttpClient _httpClient;

    public ZwiftHttpClientUnauthorized(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Stream> DownloadActivityFitFile(
        ZwiftDownloadActivityRequest model,
        CancellationToken cancellationToken)
    {
        var fitFileUrl = $"https://{model.FitFileBucket}.s3.amazonaws.com/{model.FitFileKey}";

        var response = await _httpClient.GetAsync(fitFileUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}
