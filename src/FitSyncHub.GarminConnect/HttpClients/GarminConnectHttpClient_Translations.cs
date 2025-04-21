namespace FitSyncHub.GarminConnect.HttpClients;

public partial class GarminConnectHttpClient
{
    public async Task<Dictionary<string, string>> GetExerciseTypesTranslations(CancellationToken cancellationToken = default)
    {
        const string Url = "/web-translations/exercise_types/exercise_types.properties";

        var response = await _httpClient.GetAsync(Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var result = new Dictionary<string, string>();

        foreach (var line in content.Split(['\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Split(['='], StringSplitOptions.RemoveEmptyEntries) is [string key, var value])
            {
                result.Add(key, value);
            }
        }

        return result;
    }
}
