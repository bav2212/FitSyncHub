using Riok.Mapperly.Abstractions;
using StravaWebhooksAzureFunctions.Data.Entities;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activities;

namespace StravaWebhooksAzureFunctions.Mappers;

[Mapper]
internal partial class SummaryActivityMapper
{
    [MapProperty(nameof(SummaryActivityModelResponse.Id), nameof(SummaryActivityData.id))]
    public partial SummaryActivityData SummaryActivityResponseToDataModel(SummaryActivityModelResponse activity);

    [MapProperty(nameof(ActivityModelResponse.Id), nameof(SummaryActivityData.id))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.AvailableZones))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.AverageTemp))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.Calories))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.Description))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.DeviceName))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.DisplayHideHeartrateOption))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.ElevationHigh))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.ElevationLow))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.EmbedToken))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.FromAcceptedTag))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.HeartrateOptOut))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.HideFromHome))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.PerceivedExertion))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.Photos))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.PreferPerceivedExertion))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.PrivateNote))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.SegmentEfforts))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.SplitsMetric))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.SplitsStandard))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.StatsVisibility))]
    [MapperIgnoreSource(nameof(ActivityModelResponse.UploadIdStr))]
    [MapperIgnoreTarget(nameof(SummaryActivityData.AverageHeartrate))]
    [MapperIgnoreTarget(nameof(SummaryActivityData.MaxHeartrate))]
    public partial SummaryActivityData ActivityResponseToSummaryDataModel(ActivityModelResponse activity);
}
