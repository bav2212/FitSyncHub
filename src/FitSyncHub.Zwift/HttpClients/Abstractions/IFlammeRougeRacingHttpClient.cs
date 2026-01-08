using FitSyncHub.Zwift.Models.FRR;

namespace FitSyncHub.Zwift.HttpClients.Abstractions;

public interface IFlammeRougeRacingHttpClient
{
    Task<List<long>> GetTourRegisteredRiders(
        FlammeRougeRacingCategory flammeRougeRacingCategory,
        CancellationToken cancellationToken);

    Task<List<FlammeRougeRacingEGapResultModel>> GetStageEGap(
        FlammeRougeRacingCategory flammeRougeRacingCategory,
        int stageNumber,
        CancellationToken cancellationToken);
}
