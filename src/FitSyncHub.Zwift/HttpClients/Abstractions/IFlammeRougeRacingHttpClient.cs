using FitSyncHub.Zwift.Models.FRR;

namespace FitSyncHub.Zwift.HttpClients.Abstractions;

public interface IFlammeRougeRacingHttpClient
{
    Task<List<long>> GetTourRegisteredRiders(
        FlammeRougeRacingCategory flammeRougeRacingCategory,
        CancellationToken cancellationToken);

    Task<List<FlammeRougeRacingEGapResultModel>> GetYellowJerseyStandings(
        FlammeRougeRacingCategory flammeRougeRacingCategory,
        int stageNumber,
        CancellationToken cancellationToken);

    Task<List<FlammeRougeRacingPointsResultModel>> GetPolkaDotStandings(
        FlammeRougeRacingCategory flammeRougeRacingCategory,
        int stageNumber,
        CancellationToken cancellationToken);

    Task<List<FlammeRougeRacingPointsResultModel>> GetGreenJerseyStandings(
        FlammeRougeRacingCategory flammeRougeRacingCategory,
        int stageNumber,
        CancellationToken cancellationToken);
}
