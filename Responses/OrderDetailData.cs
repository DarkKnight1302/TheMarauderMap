using Newtonsoft.Json;

namespace TheMarauderMap.Responses
{
    public class OrderDetailData
    {
        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("instrument_token")]
        public string InstrumentToken { get; set; }

        [JsonProperty("placed_by")]
        public string PlacedBy { get; set; }

        [JsonProperty("trading_symbol")]
        public string TradingSymbol { get; set; }

        [JsonProperty("order_type")]
        public string OrderType { get; set; }

        [JsonProperty("validity")]
        public string Validity { get; set; }

        [JsonProperty("trigger_price")]
        public decimal TriggerPrice { get; set; }

        [JsonProperty("disclosed_quantity")]
        public int DisclosedQuantity { get; set; }

        [JsonProperty("transaction_type")]
        public string TransactionType { get; set; }

        [JsonProperty("average_price")]
        public double AveragePrice { get; set; }

        [JsonProperty("filled_quantity")]
        public int FilledQuantity { get; set; }

        [JsonProperty("pending_quantity")]
        public int PendingQuantity { get; set; }

        [JsonProperty("exchange_order_id")]
        public string ExchangeOrderId { get; set; }

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("order_timestamp")]
        public string OrderTimestamp { get; set; }

        [JsonProperty("exchange_timestamp")]
        public string ExchangeTimestamp { get; set; }

        [JsonProperty("is_amo")]
        public bool IsAmo { get; set; }

        [JsonProperty("order_request_id")]
        public string OrderRequestId { get; set; }

        [JsonProperty("order_ref_id")]
        public string OrderRefId { get; set; }
    }
}
