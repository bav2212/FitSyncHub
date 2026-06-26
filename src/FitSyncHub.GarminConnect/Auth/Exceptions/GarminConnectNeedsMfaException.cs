using FitSyncHub.GarminConnect.Auth.Models;

namespace FitSyncHub.GarminConnect.Auth.Exceptions;

public sealed class GarminConnectNeedsMfaException : Exception
{
    public GarminConnectNeedsMfaException(GarminNeedsMfaClientState state)
    {
        ClientState = state;
    }

    public GarminNeedsMfaClientState ClientState { get; }
}
