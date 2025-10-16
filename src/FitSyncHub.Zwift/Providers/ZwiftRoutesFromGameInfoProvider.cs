using FitSyncHub.Zwift.HttpClients;
using FitSyncHub.Zwift.Providers.Abstractions;

namespace FitSyncHub.Zwift.Providers;

public class ZwiftRoutesFromGameInfoProvider : IZwiftRoutesProvider
{
    private readonly Dictionary<string, string> _worldFriendlyNameMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        {"", "Climb Portal"},
        {"NEWYORK", "New York"},
        {"WATOPIA", "Watopia"},
        {"SCOTLAND", "Scotland"},
        {"RICHMOND", "Richmond"},
        {"PARIS", "Paris"},
        {"MAKURIISLANDS", "Makuri Islands"},
        {"LONDON", "London"},
        {"YORKSHIRE", "Yorkshire"},
        {"FRANCE", "France"},
        {"INNSBRUCK", "Innsbruck"},
        {"CRITCITY", "Crit City"},
        {"BOLOGNATT", "Bologna"},
        {"GRAVEL MOUNTAIN", "Gravel Mountain"},
    };

    private readonly ZwiftHttpClient _zwiftHttpClient;

    public ZwiftRoutesFromGameInfoProvider(ZwiftHttpClient zwiftHttpClient)
    {
        _zwiftHttpClient = zwiftHttpClient;
    }

    public async Task<List<ZwiftDataWorldRoutePair>> GetRoutesInfo(CancellationToken cancellationToken)
    {
        var gameInfo = await _zwiftHttpClient.GetGameInfo(cancellationToken);

        return [.. gameInfo.Maps.SelectMany(world => world.Routes.Select(route => new ZwiftDataWorldRoutePair
        {
            WorldName = _worldFriendlyNameMapping[world.Name],
            Route = route,
        }))];
    }
}
