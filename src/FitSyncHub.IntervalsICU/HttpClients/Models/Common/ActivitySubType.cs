﻿using System.Text.Json.Serialization;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Common;

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<ActivitySubType>))]
public enum ActivitySubType
{
    None,
    Commute,
    Warmup,
    Cooldown,
    Race,
}
