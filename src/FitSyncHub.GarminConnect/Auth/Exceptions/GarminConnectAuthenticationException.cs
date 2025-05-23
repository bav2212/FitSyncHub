namespace FitSyncHub.GarminConnect.Auth.Exceptions;

public class GarminConnectAuthenticationException : Exception
{
    public GarminConnectAuthenticationException(string message) : base(message)
    {
    }

    public GarminConnectAuthenticationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public GarminConnectAuthenticationException()
    {
    }
}
