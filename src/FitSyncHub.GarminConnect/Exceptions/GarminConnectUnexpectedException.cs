namespace FitSyncHub.GarminConnect.Exceptions;

public class GarminConnectUnexpectedException : Exception
{
    public GarminConnectUnexpectedException(string property) : base($"Model changed. {property} not found!")
    {
    }
}
