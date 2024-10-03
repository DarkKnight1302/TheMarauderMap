using Newtonsoft.Json;

namespace TheMarauderMap.Responses
{
    public class SellOrderDetails
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public OrderData Data { get; set; }
    }
}
