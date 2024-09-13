using TheMarauderMap.ApiClient;
using TheMarauderMap.Entities;
using TheMarauderMap.Extensions;
using TheMarauderMap.Repositories;
using TheMarauderMap.Responses;

namespace TheMarauderMap.Services
{
    public class StockTradeService : IStockTradeService
    {
        private readonly IActiveStockRepository activeStockRepository;
        private readonly ISessionRepository sessionRepository;
        private readonly ILogger<StockTradeService> logger;
        private readonly IUpstoxApiClient upstoxApiClient;

        public StockTradeService(IActiveStockRepository activeStockRepository,
            ISessionRepository sessionRepository,
            ILogger<StockTradeService> logger,
            IUpstoxApiClient upstoxApiClient)
        {
            this.activeStockRepository = activeStockRepository;
            this.sessionRepository = sessionRepository;
            this.logger = logger;
            this.upstoxApiClient = upstoxApiClient;
        }

        public async Task PurchaseStock(string sessionId, Stock stock, int quantity, double price)
        {
            Session userSession = await this.sessionRepository.GetSession(sessionId);
            if (userSession == null)
            {
                this.logger.LogError("Session Not found");
                return;
            }
            await this.activeStockRepository.BuyStock(userSession.UserId, stock, price, quantity);
        }

        public async Task SellStock(ActiveStock stock, double sellingPrice)
        {
            await this.activeStockRepository.SellStock(stock, sellingPrice);
        }

        public async Task<PurchasedStock> StockToSell(string sessionId)
        {
            Session userSession = await this.sessionRepository.GetSession(sessionId);
            if (userSession == null)
            {
                this.logger.LogError("Session Not found");
                throw new InvalidOperationException("Session Not found");
            }
            var activeStocks = await this.activeStockRepository.GetAllActiveStocksAsync(userSession.UserId);
            if (activeStocks == null || activeStocks.Count == 0)
            {
                return null;
            }
            List<string> stockIds = activeStocks.Select(x => x.StockId).ToList();
            Dictionary<string, double> priceDictionary = await this.upstoxApiClient.GetStockPrice(stockIds, userSession.Accesstoken);
            DateTimeOffset currentDate = DateTimeOffset.UtcNow.ToIndiaTime();
            foreach (var activeStock in activeStocks)
            {
                double gainPercent = (100d * (priceDictionary[activeStock.StockId] - activeStock.BuyPrice)) / activeStock.BuyPrice;
                if ((currentDate - activeStock.BuyTime).Days > 20
                    && priceDictionary.ContainsKey(activeStock.StockId)
                    && priceDictionary[activeStock.StockId] > activeStock.BuyPrice)
                {
                    PurchasedStock purchasedStock = new PurchasedStock(activeStock);
                    purchasedStock.CurrentPrice = priceDictionary[activeStock.StockId];
                    purchasedStock.GainPercent = gainPercent;
                    return purchasedStock;
                }

                if (gainPercent > 15)
                {
                    PurchasedStock purchasedStock = new PurchasedStock(activeStock);
                    purchasedStock.CurrentPrice = priceDictionary[activeStock.StockId];
                    purchasedStock.GainPercent = gainPercent;
                    return purchasedStock;
                }
            }
            return null;
        }
    }
}
