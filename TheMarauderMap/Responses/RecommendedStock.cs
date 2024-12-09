using TheMarauderMap.Entities;

namespace TheMarauderMap.Responses
{
    public class RecommendedStock : Stock
    {
        public double CurrentPrice { get; set; }

        public double GainPercent { get; set; }

        public double LastPrice { get; set; }
    }
}
