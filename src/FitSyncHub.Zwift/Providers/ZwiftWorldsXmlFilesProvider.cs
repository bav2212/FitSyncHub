using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Zwift.Providers;

public class ZwiftWorldsXmlFilesProvider
{
    private static readonly Dictionary<string, string> s_worldIdToNameMapping = new()
    {
        {"1",  "Watopia"},
        {"2",  "Richmond"},
        {"3",  "London"},
        {"4",  "New York"},
        {"5",  "Innsbruck"},
        {"6",  "Bologna"},
        {"7",  "Yorkshire"},
        {"8",  "Crit City"},
        {"9",  "Makuri Islands"},
        {"10",  "France"},
        {"11",  "Paris"},
        {"12",  "Gravel Mountain"},
        {"13",  "Scotland"},
    };

    private readonly string _zwiftWorldsPath = @"C:\Program Files (x86)\Zwift\assets\Worlds";

    private readonly ZwiftWadDecoder _zwiftWadDecoder;
    private readonly ILogger<ZwiftWorldsXmlFilesProvider> _logger;

    private readonly string _unpackedWADFilesDirectory;
    private readonly string _unpackedWADFilesStateFilePath;

    public ZwiftWorldsXmlFilesProvider(
        ZwiftWadDecoder zwiftWadDecoder,
        ILogger<ZwiftWorldsXmlFilesProvider> logger)
    {
        _zwiftWadDecoder = zwiftWadDecoder;
        _logger = logger;

        _unpackedWADFilesDirectory = Path.Combine(Path.GetTempPath(), "ZwiftWADFilesUnpacked");
        _unpackedWADFilesStateFilePath = Path.Combine(_unpackedWADFilesDirectory, "state.json");
    }

    public async Task<List<ZwiftWorldsXmlFilesModel>> GetWorlsXmlFilesPaths(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_unpackedWADFilesDirectory))
        {
            Directory.CreateDirectory(_unpackedWADFilesDirectory);
        }

        await UnpackWADFiles(cancellationToken);

        var worldRouteFilePaths = Directory.EnumerateFiles(
            Path.Combine(_unpackedWADFilesDirectory, "Worlds"),
            "*.xml",
            new EnumerationOptions { RecurseSubdirectories = true })
            .Where(filePath =>
            {
                var pathParts = GetRelativePathParts(filePath);

                return pathParts[0].StartsWith("world", StringComparison.OrdinalIgnoreCase)
                    && pathParts[1] == "routes";
            });

        // TODO
        // If need climb portal, use reference https://github.com/zoffline/zwift-offline/blob/master/scripts/get_climbs.py as reference


        return [.. worldRouteFilePaths
            .GroupBy(GetWorldIdFromFilePath)
            .Select(x => new ZwiftWorldsXmlFilesModel
            {
                WorldId = x.Key,
                WorldName = s_worldIdToNameMapping[x.Key],
                FilePaths = [.. x]
            })];
    }

    private string GetWorldIdFromFilePath(string x)
    {
        return GetRelativePathParts(x)[0][5..]; // trim "world"
    }

    private async Task UnpackWADFiles(CancellationToken cancellationToken)
    {
        Dictionary<string, UnpackedWADFilesStateItem> unpackedWADFilesStateDictionary = [];
        if (File.Exists(_unpackedWADFilesStateFilePath))
        {
            var unpackedWADFilesStateJson = await File.ReadAllTextAsync(_unpackedWADFilesStateFilePath, cancellationToken);
            var unpackedWADFilesStateList = JsonSerializer
                .Deserialize<List<UnpackedWADFilesStateItem>>(unpackedWADFilesStateJson)!;
            unpackedWADFilesStateDictionary = unpackedWADFilesStateList
                .ToDictionary(x => x.FilePath, StringComparer.OrdinalIgnoreCase);
        }

        //inspired by https://github.com/zoffline/zwift-offline/blob/master/scripts/get_start_lines.py#L29
        // do it inside 'if block' cause service is singleton and _unpackedWADFilesDirectory will be the same during application run
        foreach (var filePath in Directory.EnumerateFiles(_zwiftWorldsPath,
            "data_1.wad", new EnumerationOptions() { RecurseSubdirectories = true }))
        {
            var hash = ComputeHash(filePath);
            if (unpackedWADFilesStateDictionary.TryGetValue(filePath, out var unpackedWADFilesStateItem)
                && unpackedWADFilesStateItem.Hash == hash)
            {
                continue;
            }

            _zwiftWadDecoder.Unpack(filePath, _unpackedWADFilesDirectory);
            unpackedWADFilesStateDictionary[filePath] = new UnpackedWADFilesStateItem { FilePath = filePath, Hash = hash };
        }

        var updatedUnpackedWADFilesStateList = JsonSerializer.Serialize(unpackedWADFilesStateDictionary.Values.ToList())!;
        await File.WriteAllTextAsync(_unpackedWADFilesStateFilePath, updatedUnpackedWADFilesStateList, cancellationToken);
    }

    // move to common if need
    private static string ComputeHash(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var myHash = md5.ComputeHash(stream);

        return Convert.ToBase64String(myHash);
    }

    private string[] GetRelativePathParts(string filePath)
    {
        var basePath = Path.Combine(_unpackedWADFilesDirectory, "Worlds");

        return [.. Path.GetRelativePath(basePath, filePath).Split(Path.DirectorySeparatorChar)];
    }

    private sealed record UnpackedWADFilesStateItem
    {
        public required string FilePath { get; init; }
        public required string Hash { get; init; }
    }
}

public sealed record ZwiftWorldsXmlFilesModel
{
    public required string WorldId { get; init; }
    public required string WorldName { get; init; }
    public required List<string> FilePaths { get; init; }
}
