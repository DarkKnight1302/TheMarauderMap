using Newtonsoft.Json;

namespace TheMarauderMap.Entities
{
    public class AccessToken
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public string UserId { get; set; }

        public string Accesstoken { get; set; }

        public DateTimeOffset RefreshTime { get; set; }
    }
}
