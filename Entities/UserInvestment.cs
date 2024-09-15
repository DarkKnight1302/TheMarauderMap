using Newtonsoft.Json;

namespace TheMarauderMap.Entities
{
    public class UserInvestment
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public string UserId { get; set; }

        public double TotalInvestment { get; set; } = 0d;

        public double TotalReturns { get; set; } = 0d;

        public double GainPercent => (100d * (TotalReturns - TotalInvestment)) / (TotalInvestment);

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
