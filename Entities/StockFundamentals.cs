using Newtonsoft.Json;

namespace TheMarauderMap.Entities
{
    public class StockFundamentals
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public string Uid { get; set; }

        public string Name { get; set; }

        public List<int> YearlyRevenue { get; set; } = new List<int>();

        public List<int> QuarterlyRevenue { get; set; } = new List<int>();

        public List<int> YearlyProfit { get; set; } = new List<int>();

        public List<int> QuarterlyProfit { get; set; } = new List<int>();


        public DateTimeOffset UpdatedAt;
    }
}
