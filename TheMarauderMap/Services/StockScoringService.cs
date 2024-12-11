using TheMarauderMap.Entities;
using TheMarauderMap.Repositories;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.Services
{
    public class StockScoringService : IStockScoringService
    {
        private readonly IStockFundamentalsRepository stockFundamentalsRepository;
        private readonly ILogger<StockScoringService> logger;

        public StockScoringService(IStockFundamentalsRepository stockFundamentalsRepository, ILogger<StockScoringService> logger)
        {
            this.logger = logger;
            this.stockFundamentalsRepository = stockFundamentalsRepository;
        }

        public async Task<Dictionary<string, int>> GetStockGrowthScores(List<Stock> stocks)
        {
            Dictionary<string, int> growthScoreMap = new Dictionary<string, int>();
            List<string> stockSymbolIds = stocks.Select(x => x.TradingSymbol).ToList();
            Dictionary<string, StockFundamentals> stockFundamentalsMap = await this.stockFundamentalsRepository.GetStockFundamentals(stockSymbolIds);
            foreach(Stock stock in stocks)
            {
                StockFundamentals fundamentals = stockFundamentalsMap[stock.TradingSymbol];
                int growthScore = CalculateGrowthScore(fundamentals);
                growthScoreMap.TryAdd(stock.Id, growthScore);
            }
            return growthScoreMap;
        }

        private int CalculateGrowthScore(StockFundamentals stockFundamentals)
        {
            int totalScore = 0;
            totalScore += CalculateSegmentScore(stockFundamentals.YearlyRevenue, 5, 100);
            totalScore += CalculateSegmentScore(stockFundamentals.YearlyProfit, 10, 100);
            totalScore += CalculateSegmentScore(stockFundamentals.QuarterlyRevenue, 2, 2);
            totalScore += CalculateSegmentScore(stockFundamentals.QuarterlyProfit, 2, 2);
            return totalScore;
        }

        private int CalculateSegmentScore(List<int> data, int growthWeight, int penaltyWeight)
        {
            int score = 0;
            for(int i=1; i<data.Count; i++)
            {
                int changePercent = Math.Abs(data[i] * 100);
                if (data[i - 1] != 0)
                {
                    changePercent = Math.Abs((Math.Abs(data[i] - data[i - 1]) * 100) / data[i - 1]);
                }
                if (data[i] > data[i-1])
                {
                    score += growthWeight * changePercent;
                } else
                {
                    score -= penaltyWeight * changePercent;
                }
            }
            return score;
        }
    }
}
