using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Zwift.Services;

public sealed class ZwiftPowerService
{
    private readonly ILogger<ZwiftPowerService> _logger;

    public ZwiftPowerService(ILogger<ZwiftPowerService> logger)
    {
        _logger = logger;
    }

    public async Task<HashSet<int>> GetTeamUkraineRidersIds()
    {
        _logger.LogInformation("Loading Team Ukraine riders IDs from files");

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
