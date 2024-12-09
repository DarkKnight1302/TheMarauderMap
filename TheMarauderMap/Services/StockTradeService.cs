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
        private readonly IUserInvestmentsRepository userInvestmentsRepository;
        private readonly IUserBlackListRepository userBlackListRepository;
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public StockTradeService(IActiveStockRepository activeStockRepository,
            ISessionRepository sessionRepository,
            ILogger<StockTradeService> logger,
            IUpstoxApiClient upstoxApiClient,
            IUserInvestmentsRepository userInvestmentsRepository,
            IUserBlackListRepository userBlackListRepository)
        {
            this.activeStockRepository = activeStockRepository;
            this.sessionRepository = sessionRepository;
            this.logger = logger;
            this.upstoxApiClient = upstoxApiClient;
            this.userInvestmentsRepository = userInvestmentsRepository;
            this.userBlackListRepository = userBlackListRepository;
        }

        public async Task BlackListStock(string sessionId, Stock stock)
        {
            Session userSession = await this.sessionRepository.GetSession(sessionId);
            if (userSession == null)
            {
                this.logger.LogError("Session Not found");
                return;
            }
            await this.userBlackListRepository.AddStockToBlackList(userSession.UserId, stock.Id);
        }

        public async Task<List<PurchasedStock>> GetAllActiveStocks(string sessionId)
        {
            Session userSession = await this.sessionRepository.GetSession(sessionId);
            if (userSession == null)
            {
                this.logger.LogError("Session Not found");
                return null;
            }
            List<ActiveStock> activeStocks = await this.activeStockRepository.GetAllActiveStocksAsync(userSession.UserId);
            if (activeStocks == null || activeStocks.Count == 0)
            {
                return null;
            }
            List<string> stockIds = activeStocks.Select(x => x.StockId).ToList();
            Dictionary<string, double> priceDictionary = await this.upstoxApiClient.GetStockPrice(stockIds, userSession.Accesstoken);
            DateTimeOffset currentDate = DateTimeOffset.UtcNow.ToIndiaTime();
            List<PurchasedStock> purchasedStocks = new List<PurchasedStock>();
            foreach (var activeStock in activeStocks)
            {
                PurchasedStock purchased = new PurchasedStock(activeStock);
                if (priceDictionary.ContainsKey(activeStock.StockId))
                {
                    double gainPercent = (100d * (priceDictionary[activeStock.StockId] - activeStock.BuyPrice)) / activeStock.BuyPrice;
                    purchased.CurrentPrice = priceDictionary[activeStock.StockId];
                    purchased.GainPercent = ((int)gainPercent);
                }
                purchasedStocks.Add(purchased);
            }
            return purchasedStocks;
        }

        public async Task<List<PurchasedStock>> GetAllInActiveStocks(string sessionId)
        {
            Session userSession = await this.sessionRepository.GetSession(sessionId);
            if (userSession == null)
            {
                this.logger.LogError("Session Not found");
                return null;
            }
            List<ActiveStock> inActiveStocks = await this.activeStockRepository.GetAllInActiveStocksAsync(userSession.UserId);
            if (inActiveStocks == null || inActiveStocks.Count == 0)
            {
                return null;
            }
            List<PurchasedStock> purchasedStocks = new List<PurchasedStock>();
            foreach (var inActiveStock in inActiveStocks)
            {
                PurchasedStock purchased = new PurchasedStock(inActiveStock);
                purchased.GainPercent = Math.Round((100 * (purchased.SellPrice - purchased.BuyPrice) / purchased.BuyPrice), 1);
                purchased.GainAmount = Math.Round((purchased.SellPrice * purchased.Quantity) - (purchased.BuyPrice * purchased.Quantity), 1);
                purchasedStocks.Add(purchased);
            }
            purchasedStocks.Sort((PurchasedStock a, PurchasedStock b) =>
            {
                return b.SellTime.CompareTo(a.SellTime);
            });
            return purchasedStocks;
        }

        public async Task<bool> PurchaseStock(string sessionId, Stock stock, int quantity, double price)
        {
            try
            {
                Session userSession = await this.sessionRepository.GetSession(sessionId);
                if (userSession == null)
                {
                    this.logger.LogError("Session Not found");
                    return false;
                }
                await this.activeStockRepository.BuyStock(userSession.UserId, stock, price, quantity);
                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"{ex.Message} : {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> SellStock(ActiveStock stock, double sellingPrice)
        {
            try
            {
                await semaphoreSlim.WaitAsync();
                bool exists = await this.activeStockRepository.ActiveStockExists(stock.Id, stock.StockId);
                if (exists)
                {
                    await this.activeStockRepository.SellStock(stock, sellingPrice);
                    double invested = stock.BuyPrice * stock.Quantity;
                    double returns = sellingPrice * stock.Quantity;
                    await this.userInvestmentsRepository.AddOrUpdateInvestment(invested, returns, stock.UserId);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return false;
            }
            finally
            {
                this.semaphoreSlim.Release();
            }
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
