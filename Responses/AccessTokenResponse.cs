using Newtonsoft.Json;

namespace TheMarauderMap.Responses
{
    public class AccessTokenResponse
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("exchanges")]
        public List<string> Exchanges { get; set; }

        [JsonProperty("products")]
        public List<string> Products { get; set; }

        [JsonProperty("broker")]
        public string Broker { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("order_types")]
        public List<string> OrderTypes { get; set; }

        [JsonProperty("user_type")]
        public string UserType { get; set; }

        [JsonProperty("poa")]
        public bool Poa { get; set; }

        [JsonProperty("is_active")]
        public bool IsActive { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("extended_token")]
        public string ExtendedToken { get; set; }
    }
}
