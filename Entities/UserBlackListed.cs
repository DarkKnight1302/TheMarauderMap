using Newtonsoft.Json;

namespace TheMarauderMap.Entities
{
    public class UserBlackListed
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public string UserId { get; set; }

        public List<string> BlackListedStocks { get; set; } = new List<string>();
    }
}
