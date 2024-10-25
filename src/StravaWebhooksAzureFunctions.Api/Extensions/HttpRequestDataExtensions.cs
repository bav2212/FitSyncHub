using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace StravaWebhooksAzureFunctions.Extensions;

internal static class HttpRequestDataExtensions
{
    internal static HttpResponseData CreateBadRequest(this HttpRequestData req, string plainText)
    {
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.WriteString(plainText);

        return response;
    }
}
