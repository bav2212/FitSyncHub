using System.Text.Json;

namespace FitSyncHub.Zwift.Helpers;

public static class ZwiftPowerHelper
{
    public static async Task<HashSet<int>> GetTeamUkraineRidersIds()
    {
        var tasks = Directory.EnumerateFiles("Files/ZwiftPowerTeams", "*.json")
            .Select(GetIdsFromFile);
        var executedTasks = await Task.WhenAll(tasks);
        return [.. executedTasks.SelectMany(x => x)];
    }

    private static async Task<List<int>> GetIdsFromFile(string fileName)
    {
        var json = await File.ReadAllTextAsync(fileName);
        var jsonDocument = JsonDocument.Parse(json);

        return [.. jsonDocument.RootElement.GetProperty("data")
            .EnumerateArray()
            .Select(x => x.GetProperty("zwid").GetInt32())];
    }
}
