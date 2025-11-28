using System.Text.Json;

namespace FitSyncHub.GarminConnect.HttpClients;

public partial class GarminConnectHttpClient
{
    public Task<int> GetCyclingFtp(CancellationToken cancellationToken)
    {
        return GetCyclingFtp(null, cancellationToken);
    }

    public async Task<int> GetCyclingFtp(
        DateOnly? date,
        CancellationToken cancellationToken)
    {
        date ??= DateOnly.FromDateTime(DateTime.UtcNow);

        var url = $"/biometric-service/biometric/powerToWeight/latest/{date:yyyy-MM-dd}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var jsonDocument = JsonDocument.Parse(responseContent);

        return jsonDocument.RootElement
            .EnumerateArray()
            .Where(x => x.GetProperty("sport").GetString() == "CYCLING")
            .Select(x => x.GetProperty("functionalThresholdPower").GetInt32())
            .Single();
    }
}
