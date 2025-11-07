using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.GarminConnect.HttpClients;

public partial class GarminConnectHttpClient
{
    public async Task<int> GetCyclingFtp(
        DateOnly? date = default,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, StringValues> queryParams = new()
        {
            { "date", $"{date ?? DateOnly.FromDateTime(DateTime.UtcNow):yyyy-MM-dd}" },
        };

        var url = QueryHelpers.AddQueryString(
            "/biometric-service/biometric/powerToWeight/latest", queryParams);

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
