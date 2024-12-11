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
        private readonly IUserBlackListRepository _userBlackListRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IStockScoringService scoringService;

        public StockRecommendationService(IStockRepository stockRepository,
            IUpstoxApiClient upstoxApiClient,
            IAccessTokenService accessTokenService,
            IUserBlackListRepository userBlackListRepository,
            ISessionRepository sessionRepository,
            IStockScoringService stockScoringService)
        {
            this._stockRepository = stockRepository;
            this._upstoxApiClient = upstoxApiClient;
            this._accessTokenService = accessTokenService;
            this._userBlackListRepository = userBlackListRepository;
            this._sessionRepository = sessionRepository;
            this.scoringService = stockScoringService;
        }

        public async Task<List<RecommendedStock>> RecommendStocks(string sessionId)
        {
            List<Stock> stocks = await this._stockRepository.GetAllStocks();
            Session session = await this._sessionRepository.GetSession(sessionId);
            if (session == null)
            {
                throw new UnauthorizedAccessException("Invalid Session Id");
            }
            string userId = session.UserId;

            List<string> blackListed = await this._userBlackListRepository.GetBlackListedStocks(userId);
            List<RecommendedStock> recommendedStocks = new List<RecommendedStock>();
            HashSet<string> blackListHashSet = blackListed.ToHashSet();
            foreach (Stock stock in stocks)
            {
                if (blackListHashSet.Contains(stock.Id) || FilterOutStock(stock))
                {
                    continue;
                }
                RecommendedStock recommendedStock = new RecommendedStock();
                recommendedStock.Id = stock.Id;
                recommendedStock.Name = stock.Name;
                recommendedStock.PriceHistory = stock.PriceHistory;
                recommendedStock.TradingSymbol = stock.TradingSymbol;
                recommendedStock.Uid = stock.Uid;
                recommendedStocks.Add(recommendedStock);
            }
            recommendedStocks = await this.GetTop10Stocks(recommendedStocks);
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

        private bool FilterOutStock(Stock stock)
        {
            if (stock == null || stock.PriceHistory.Count < 3)
            {
                return true;
            }
            int ct = stock.PriceHistory.Count;
            double c = stock.PriceHistory[ct - 1].Price;
            double b = stock.PriceHistory[ct - 2].Price;
            double a = stock.PriceHistory[ct - 3].Price;
            if (a < b && b < c)
            {
                return false;
            }
            return true;
        }

        private async Task<List<RecommendedStock>> GetTop10Stocks(List<RecommendedStock> recommendedStocks)
        {
            recommendedStocks.Sort(Compare);
            recommendedStocks = recommendedStocks.Take(50).ToList();
            List<Stock> stockObjects = recommendedStocks.Select(x => (Stock)x).ToList();
            Dictionary<string, int> stockGrowthScoreMap = await this.scoringService.GetStockGrowthScores(stockObjects);
            foreach(RecommendedStock recommendedStock in recommendedStocks)
            {
                recommendedStock.Score = stockGrowthScoreMap[recommendedStock.Id];
            }
            recommendedStocks.Sort((a, b) =>
            {
                int ascore = a.Score;
                int bscore = b.Score;
                return bscore.CompareTo(ascore);
            });
            return recommendedStocks.Take(10).ToList();
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
