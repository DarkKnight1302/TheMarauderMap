using Newtonsoft.Json;

namespace TheMarauderMap.Responses
{
    public class OrderData
    {
        [JsonProperty("order_id")]
        public string OrderId { get; set; }
    }
}
