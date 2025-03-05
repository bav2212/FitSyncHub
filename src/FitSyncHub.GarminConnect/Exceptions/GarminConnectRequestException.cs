using System.Net;

namespace FitSyncHub.GarminConnect.Exceptions;

public class GarminConnectRequestException : Exception
{
    public string Url { get; }

    public HttpStatusCode Status { get; }

    public GarminConnectRequestException(string url, HttpStatusCode status) : base(
        $"Request [{url}] return code {(int)status} ({status.ToString()}).")
    {
        Url = url;
        Status = status;
    }
}
