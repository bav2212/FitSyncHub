using Garmin.Connect;

namespace FitSyncHub.GarminConnect;

public class ExtendedGarminConnectClient : GarminConnectClient
{
    private readonly GarminConnectContext _context;

    public ExtendedGarminConnectClient(GarminConnectContext context) : base(context)
    {
        _context = context;
    }

    public async Task<HttpResponseMessage> UpdateActivity(GarminActivityUpdateRequest model, CancellationToken cancellationToken)
    {
        var url = $"/activity-service/activity/{model.ActivityId}";

        var headers = new Dictionary<string, string> { { "X-Http-Method-Override", "PUT" } };
        var response = await _context.MakeHttpPost(url, model, headers, cancellationToken);
        return response;
    }
}
