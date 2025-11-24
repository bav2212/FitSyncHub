namespace FitSyncHub.GarminConnect;

// do not change namespace, cause azure alerts are using this exception
// https://portal.azure.com/#@bav2212gmail.onmicrosoft.com/resource/subscriptions/b08b8c93-c32d-4e9c-bb3a-1720a5326ac1/resourceGroups/FitSyncHub/providers/microsoft.insights/scheduledqueryrules/GarminLoggedIn/overview
public sealed class GarminNotLoggedInException : Exception
{
    public GarminNotLoggedInException() : base("Token is invalid. Re-login to Garmin") { }

    public GarminNotLoggedInException(string message) : base(message) { }
    public GarminNotLoggedInException(string message, Exception inner) : base(message, inner) { }
}
