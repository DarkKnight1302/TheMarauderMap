namespace TheMarauderMap.Responses
{
    using Newtonsoft.Json;

    public class Instrument
    {
        [JsonProperty("segment")]
        public string Segment { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("isin")]
        public string ISIN { get; set; }

        [JsonProperty("instrument_type")]
        public string InstrumentType { get; set; }

        [JsonProperty("instrument_key")]
        public string InstrumentKey { get; set; }

        [JsonProperty("lot_size")]
        public int LotSize { get; set; }

        [JsonProperty("freeze_quantity")]
        public double FreezeQuantity { get; set; }

        [JsonProperty("exchange_token")]
        public string ExchangeToken { get; set; }

        [JsonProperty("tick_size")]
        public double TickSize { get; set; }

        [JsonProperty("trading_symbol")]
        public string TradingSymbol { get; set; }

        [JsonProperty("security_type")]
        public string SecurityType { get; set; }
    }

}
