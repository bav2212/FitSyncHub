﻿using System.Text.Json.Serialization;
using FitSyncHub.GarminConnect.Models.Responses.TrainingPlan;

namespace FitSyncHub.GarminConnect.JsonSerializerContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(GarminTrainingPlanResponse))]
internal partial class GarminConnectTrainingPlanSerializerContext : JsonSerializerContext;
