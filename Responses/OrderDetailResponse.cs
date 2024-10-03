using Newtonsoft.Json;

namespace TheMarauderMap.Responses
{
    public class OrderDetailResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public OrderDetailData Data { get; set; }
    }
}
