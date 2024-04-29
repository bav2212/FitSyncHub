using Riok.Mapperly.Abstractions;
using StravaWebhooksAzureFunctions.Data.Entities;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activities;

namespace StravaWebhooksAzureFunctions.Mappers;

[Mapper]
internal partial class SummaryActivityMapper
{
    [MapProperty(nameof(SummaryActivityModelResponse.Id), nameof(SummaryActivityData.id))]
    public partial SummaryActivityData SummaryActivityResponseToDataModel(SummaryActivityModelResponse activity);
}
