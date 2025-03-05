﻿namespace FitSyncHub.GarminConnect.Options;

public record GarminConnectAuthOptions
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}
