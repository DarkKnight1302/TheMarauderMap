using TheMarauderMap.ApiClient;
using TheMarauderMap.Entities;
using TheMarauderMap.Repositories;
using TheMarauderMap.Responses;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.Services
{
    public class StockRecommendationService : IStockRecommendationService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IAccessTokenService _accessTokenService;
        private readonly IUpstoxApiClient _upstoxApiClient;

        public StockRecommendationService(IStockRepository stockRepository, IUpstoxApiClient upstoxApiClient, IAccessTokenService accessTokenService)
        {
            this._stockRepository = stockRepository;
            this._upstoxApiClient = upstoxApiClient;
            this._accessTokenService = accessTokenService;
        }

        public async Task<List<RecommendedStock>> RecommendStocks(string sessionId)
        {
            List<Stock> stocks = await this._stockRepository.GetAllStocks();
            List<RecommendedStock> recommendedStocks = new List<RecommendedStock>();
            foreach (Stock stock in stocks)
            {
                RecommendedStock recommendedStock = new RecommendedStock();
                recommendedStock.Id = stock.Id;
                recommendedStock.Name = stock.Name;
                recommendedStock.PriceHistory = stock.PriceHistory;
                recommendedStock.TradingSymbol = stock.TradingSymbol;
                recommendedStock.Uid = stock.Uid;
                recommendedStocks.Add(recommendedStock);
            }
            recommendedStocks.Sort(Compare);
            recommendedStocks = recommendedStocks.Take(10).ToList();
            List<string> stocksIds = recommendedStocks.Select(x => x.Id).ToList();
            string accessToken = await this._accessTokenService.FetchAccessToken(sessionId);
            if (!string.IsNullOrEmpty(accessToken))
            {
                Dictionary<string, double> stockDictionary = await this._upstoxApiClient.GetStockPrice(stocksIds, accessToken);
                foreach (RecommendedStock re in recommendedStocks)
                {
                    if (stockDictionary.ContainsKey(re.Id))
                    {
                        re.CurrentPrice = stockDictionary[re.Id];
                    }
                }
            }
            return recommendedStocks;
        }

        public int Compare(RecommendedStock s1, RecommendedStock s2)
        {
            int n1 = s1.PriceHistory.Count;
            int n2 = s2.PriceHistory.Count;
            if (n1 < 4 || n2 < 4)
            {
                return 0;
            }
            double x1 = (s1.PriceHistory[n1 - 1].Price - s1.PriceHistory[n1 - 4].Price) / (s1.PriceHistory[n1 - 4].Price);
            double x2 = (s2.PriceHistory[n2 - 1].Price - s2.PriceHistory[n2 - 4].Price) / (s2.PriceHistory[n2 - 4].Price);
            s1.GainPercent = Math.Round(x1 * 100, 1);
            s1.LastPrice = s1.PriceHistory[n1 - 1].Price;
            s2.LastPrice = s2.PriceHistory[n2 - 1].Price;
            s2.GainPercent = Math.Round(x2 * 100, 1);
            return x2.CompareTo(x1);
        }
    }
}
