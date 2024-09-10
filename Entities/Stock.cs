using Newtonsoft.Json;

namespace TheMarauderMap.Entities
{
    public class Stock
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public string Uid { get; set; }

        public string Name { get; set; }

        public string TradingSymbol { get; set; }

        public List<StockPrice> PriceHistory { get; set; } = new List<StockPrice>();
    }

    public class StockPrice
    {
        public DateTimeOffset Date { get; set; }

        public double Price { get; set; }
    }
}
