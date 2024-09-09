using Newtonsoft.Json;

namespace TheMarauderMap.Entities
{
    public class Session
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public string SessionId { get; set; }

        public string Accesstoken { get; set; }

        public string UserId { get; set; }

        public DateTimeOffset CreationTime { get; set; }
    }
}
