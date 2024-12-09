namespace TheMarauderMap.Models
{
    public class StockFundamentalsResp
    {
        public List<int> YearlyRevenue { get; set; }

        public List<int> QuarterlyRevenue { get; set; }

        public List<int> YearlyProfit { get; set; }

        public List<int> QuarterlyProfit { get; set; }
    }
}
