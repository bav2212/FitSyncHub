using System.Text.Json.Serialization;

namespace FitSyncHub.Zwift.HttpClients.Models.Responses
{
    public record ZwiftEventResponse
    {
        [JsonConverter(typeof(DateTimeWithoutColonOffsetJsonConverter))]
        public DateTime EventStart { get; set; }
        public ZwiftEventSubgroupResponse[] EventSubgroups { get; set; }
    }

    public class ZwiftEventSubgroupResponse
    {
        public int Id { get; set; }
        public string SubgroupLabel { get; set; }
        public string[] RulesSet { get; set; }
    }

}
