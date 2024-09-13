using Newtonsoft.Json;

namespace TheMarauderMap.Entities
{
    public class ActiveStock
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public string Uid { get; set; }

        public string UserId { get; set; }

        public string StockId { get; set; }

        public string Name { get; set; }

        public string TradingSymbol { get; set; }

        public bool IsActive { get; set; } = true;

        public double BuyPrice { get; set; }

        public double SellPrice { get; set; }

        public int Quantity { get; set; }

        public DateTimeOffset BuyTime { get; set; }

        public DateTimeOffset SellTime { get; set; }
    }
}
