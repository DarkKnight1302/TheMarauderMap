using Newtonsoft.Json;

namespace TheMarauderMap.Responses
{
    public class ScreenerSearchResponse
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
