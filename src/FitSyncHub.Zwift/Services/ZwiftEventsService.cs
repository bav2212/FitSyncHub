using FitSyncHub.Zwift.HttpClients;

namespace FitSyncHub.Zwift.Services;

public sealed class ZwiftEventsService
{
    private readonly ZwiftHttpClient _zwiftHttpClient;

    public ZwiftEventsService(ZwiftHttpClient zwiftHttpClient)
    {
        _zwiftHttpClient = zwiftHttpClient;
    }

    public async Task<ZwiftPlayerCompetitionMetrics> GetCompetitionMetrics(
        long id,
        CancellationToken cancellationToken)
    {
        var response = await _zwiftHttpClient.GetProfileDetailed(id, cancellationToken);

        var minor = response.Privacy is { } && response.Privacy.Minor;
        var male = response.Male;

        var (racingScore, category) = response.CompetitionMetrics switch
        {
            null => (null, null),
            { } competitionMetrics =>
            (
                competitionMetrics.RacingScore,
                minor || male ? competitionMetrics.Category : competitionMetrics.CategoryWomen
            )
        };

        return new ZwiftPlayerCompetitionMetrics
        {
            Id = id,
            FirstName = response.FirstName,
            LastName = response.LastName,
            Category = category,
            RacingScore = racingScore,
            Male = male,
        };
    }

    public Task<IReadOnlyCollection<ZwiftEntrantResponseModel>> GetEntrants(
        string zwiftEventUrl,
        string? subgroupLabel,
        CancellationToken cancellationToken)
    {
        return GetEntrants(zwiftEventUrl, subgroupLabel, includeMyself: false, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ZwiftEntrantResponseModel>> GetEntrants(
        string zwiftEventUrl,
        string? subgroupLabel,
        bool includeMyself,
        CancellationToken cancellationToken)
    {
        var zwiftEvent = await _zwiftHttpClient.GetEvent(zwiftEventUrl, cancellationToken);

        var eventSubgroupId = (string.IsNullOrWhiteSpace(subgroupLabel), zwiftEvent.EventSubgroups) switch
        {
            (true, { Length: 1 }) => zwiftEvent.EventSubgroups.Single().Id,
            (false, { Length: >= 1 }) => zwiftEvent.EventSubgroups.Single(x => x.SubgroupLabel == subgroupLabel).Id,
            _ => throw new InvalidDataException("Can not map subgroupLabel to eventSubgroup")
        };

        var entrants = await _zwiftHttpClient
            .GetEventSubgroupEntrants(eventSubgroupId, cancellationToken: cancellationToken);

        var result = entrants.Select(e => new ZwiftEntrantResponseModel
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Age = e.Age,
            Ftp = e.Ftp,
            WeightInGrams = e.Weight,
            HeightInMillimeters = e.Height,
        }).ToList();

        if (!includeMyself)
        {
            return result;
        }

        var profileMe = await _zwiftHttpClient.GetProfileMe(cancellationToken);
        if (entrants.Any(x => x.Id == profileMe.Id))
        {
            return result;
        }

        return [.. result, new ZwiftEntrantResponseModel
                {
                    Id = profileMe.Id,
                    FirstName = profileMe.FirstName,
                    LastName = profileMe.LastName,
                    Age = profileMe.Age,
                    Ftp = profileMe.Ftp,
                    WeightInGrams = profileMe.WeightInGrams,
                    HeightInMillimeters = profileMe.HeightInMillimeters,
                }];
    }
}

public sealed record ZwiftPlayerCompetitionMetrics
{
    public required long Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required bool Male { get; set; }
    public required string? Category { get; set; }
    public required double? RacingScore { get; set; }
}

public sealed record ZwiftEntrantResponseModel
{
    public required long Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required uint Age { get; set; }
    public required uint WeightInGrams { get; set; }
    public required uint HeightInMillimeters { get; set; }
    public required uint Ftp { get; set; }
}
