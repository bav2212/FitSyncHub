﻿namespace FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;

public record ZwiftGameInfoResponse
{
    public required List<ZwiftGameInfoAchievement> Achievements { get; init; }
    public required List<ZwiftGameInfoMap> Maps { get; init; }
}
