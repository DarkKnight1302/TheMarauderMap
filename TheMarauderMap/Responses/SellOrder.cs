using Newtonsoft.Json;

namespace TheMarauderMap.Responses
{
    public class SellOrder
    {
        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("validity")]
        public string Validity { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("instrument_token")]
        public string InstrumentToken { get; set; }

        [JsonProperty("order_type")]
        public string OrderType { get; set; }

        [JsonProperty("transaction_type")]
        public string TransactionType { get; set; }

        [JsonProperty("disclosed_quantity")]
        public int DisclosedQuantity { get; set; } = 0;

        [JsonProperty("trigger_price")]
        public decimal TriggerPrice { get; set; } = 0;

        [JsonProperty("is_amo")]
        public bool IsAmo { get; set; } = false;
    }
}
