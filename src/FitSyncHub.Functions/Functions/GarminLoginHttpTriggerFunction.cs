using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.Auth.Abstractions;
using FitSyncHub.GarminConnect.Auth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;


namespace FitSyncHub.Functions.Functions;

public class GarminLoginHttpTriggerFunction
{
    private readonly IGarminTokenSetter _garminTokenSetter;

    public GarminLoginHttpTriggerFunction(IGarminTokenSetter garminTokenSetter)
    {
        _garminTokenSetter = garminTokenSetter;
    }

    [Function(nameof(GarminLoginHttpTriggerFunction))]
    public async Task<ActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "garmin/login")] HttpRequest req,
        [FromBody] GarminDiTokenModelRequest request,
        CancellationToken cancellationToken)
    {
        //run https://github.com/cyberjunky/python-garminconnect locally to get token and copy response to this method body

        _ = req;

        var tokenModel = new GarminDiTokenModel
        {
            DiToken = request.DiToken,
            DiRefreshToken = request.DiRefreshToken,
            DiClientId = request.DiClientId
        };

        await _garminTokenSetter.SetTokenModel(tokenModel, cancellationToken);

        return new OkObjectResult("Ok");
    }

    public sealed record GarminDiTokenModelRequest
    {
        [JsonPropertyName("di_token")]
        public required string DiToken { get; init; }
        [JsonPropertyName("di_refresh_token")]
        public required string DiRefreshToken { get; init; }
        [JsonPropertyName("di_client_id")]
        public required string DiClientId { get; init; }
    }
}
