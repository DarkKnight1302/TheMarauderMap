using Newtonsoft.Json;

namespace TheMarauderMap.Responses
{
    public class StockApiResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public Dictionary<string, StockData> Data { get; set; }
    }

    public class StockData
    {
        [JsonProperty("last_price")]
        public double LastPrice { get; set; }

        [JsonProperty("instrument_token")]
        public string InstrumentToken { get; set; }
    }
}
