using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TheMarauderMap.Enums;

namespace TheMarauderMap.Entities
{
    public class JobExecution
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public string JobId { get; set; }

        public string JobName { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset Ended { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "Status")]
        public JobStatus Status { get; set; }

        public string FailureReason { get; set; }

        public string ExecutionFlow {  get; set; }
    }
}
