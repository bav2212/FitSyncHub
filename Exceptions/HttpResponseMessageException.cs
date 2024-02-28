using System;
using System.Net.Http;

namespace StravaWebhooksAzureFunctions.Exceptions;

public class HttpResponseMessageException : Exception
{
    public HttpResponseMessageException(string message, HttpResponseMessage responseMessage) : base(message)
    {
        ResponseMessage = responseMessage;
    }

    public HttpResponseMessage ResponseMessage { get; }
}
