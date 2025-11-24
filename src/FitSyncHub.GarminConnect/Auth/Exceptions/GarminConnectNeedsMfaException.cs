using FitSyncHub.GarminConnect.Auth.Models;

namespace FitSyncHub.GarminConnect.Auth.Exceptions;

#pragma warning disable RCS1194 // Implement exception constructors
public sealed class GarminConnectNeedsMfaException : Exception
#pragma warning restore RCS1194 // Implement exception constructors
{
    public GarminConnectNeedsMfaException(GarminNeedsMfaClientState state)
    {
        ClientState = state;
    }

    public GarminNeedsMfaClientState ClientState { get; }
}
