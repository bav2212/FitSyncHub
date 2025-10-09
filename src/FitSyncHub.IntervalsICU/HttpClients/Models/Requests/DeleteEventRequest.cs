namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public record DeleteEventRequest
{
    public DeleteEventRequest(int eventId)
    {
        EventId = eventId;
    }

    public int EventId { get; set; }
    public bool Others { get; set; }
    public DateOnly NotBefore { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}
