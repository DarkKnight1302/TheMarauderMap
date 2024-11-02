using Newtonsoft.Json;

namespace TheMarauderMap.Entities
{
    public class UserBlackListed
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public string UniqueId { get; set; }

        public string UserId { get; set; }
        public string StockId { get; set; }
    }
}
