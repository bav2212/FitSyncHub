using System.Text.Json;
using System.Text.RegularExpressions;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Events;

namespace FitSyncHub.Zwift.HttpClients;

public class ZwiftEventsService
{
    private readonly ZwiftHttpClient _zwiftHttpClient;

    public ZwiftEventsService(ZwiftHttpClient zwiftHttpClient)
    {
        _zwiftHttpClient = zwiftHttpClient;
    }

    public async Task<IReadOnlyCollection<ZwiftEntrantResponseModel>> GetEntrants(
        string zwiftEventUrl,
        string subgroupLabel,
        bool includeMyself = false,
        CancellationToken cancellationToken = default)
    {
        var zwiftEvent = await _zwiftHttpClient.GetEventFromZwfitEventViewUrl(zwiftEventUrl, cancellationToken);
        var eventSubgroupId = zwiftEvent.EventSubgroups
            .Single(x => x.SubgroupLabel == subgroupLabel).Id;

        var entrants = await _zwiftHttpClient
            .GetEventSubgroupEntrants(eventSubgroupId, cancellationToken: cancellationToken);

        var result = entrants.Select(e => new ZwiftEntrantResponseModel
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Age = e.Age,
            Ftp = e.Ftp,
            WeightInGrams = e.Weight
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
                    WeightInGrams = profileMe.WeightInGrams
                }];
    }
}

public record ZwiftEntrantResponseModel
{
    public required long Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required uint Age { get; set; }
    public required uint WeightInGrams { get; set; }
    public required uint Ftp { get; set; }
}
