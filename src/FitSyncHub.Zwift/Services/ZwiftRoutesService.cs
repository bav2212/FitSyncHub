using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using FitSyncHub.Zwift.Providers.Abstractions;

namespace FitSyncHub.Zwift.Services;

public class ZwiftRoutesService
{
    private readonly IZwiftRoutesProvider _zwiftRoutesProvider;

    public ZwiftRoutesService(IZwiftRoutesProvider zwiftRoutesProvider)
    {
        _zwiftRoutesProvider = zwiftRoutesProvider;
    }

    public async Task<List<ZwiftDataRoutesInfoModel>> GetRoutesInfo(CancellationToken cancellationToken)
    {
        var worldRoutePairs = await _zwiftRoutesProvider.GetRoutesInfo(cancellationToken);

        var result = worldRoutePairs
          .Where(x => x.Route.Sports.Contains(ZwiftGameInfoSport.Cycling))
          .Select(pair =>
          {
              var route = pair.Route;

              var restrictions = new List<string>();
              if (route.Sports.Count == 1 && route.Sports.Contains(ZwiftGameInfoSport.Running))
              {
                  restrictions.Add("Run Only");
              }

              if (route.PublicEventsOnly)
              {
                  restrictions.Add("Event Only");
              }

              if (route.LevelLocked > 1)
              {
                  restrictions.Add($"Level {route.LevelLocked}+");

              }

              return new ZwiftDataRoutesInfoModel
              {
                  Name = route.Name,
                  WorldName = pair.WorldName,
                  Distance = Math.Round(route.DistanceInMeters / 1000, 1),
                  ElevationGain = Math.Round(route.AscentInMeters),
                  LeadIn = Math.Round(route.LeadinDistanceInMeters / 1000, 1),
                  LeadInElevationGain = Math.Round(route.LeadinAscentInMeters),
                  Restrictions = restrictions.Count != 0 ? string.Join(", ", restrictions) : null,
              };
          });

        return [.. result
            .OrderBy(x => x.WorldName)
            .ThenBy(x => x.Name)
        ];
    }
}


public record ZwiftDataRoutesInfoModel
{
    public required string Name { get; init; }
    public required string WorldName { get; init; }
    public required double Distance { get; init; }
    public required double ElevationGain { get; init; }
    public required double LeadIn { get; init; }
    public required double LeadInElevationGain { get; init; }
    public required string? Restrictions { get; init; }
}
