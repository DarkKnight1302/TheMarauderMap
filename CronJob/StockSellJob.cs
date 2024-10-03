using Microsoft.Extensions.Caching.Memory;
using Quartz;
using TheMarauderMap.ApiClient;
using TheMarauderMap.Entities;
using TheMarauderMap.Extensions;
using TheMarauderMap.Repositories;
using TheMarauderMap.Responses;
using TheMarauderMap.Services;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.CronJob
{
    public class StockSellJob : IJob
    {
        private readonly ILogger<StockSellJob> _logger;
        private readonly IAccessTokenService _accessTokenService;
        private readonly IActiveStockRepository activeStockRepository;
        private readonly IUpstoxApiClient upstoxApiClient;
        private readonly IMemoryCache memoryCache;
        private readonly IStockTradeService stockTradeService;
        private readonly IJobExecutionRepository jobExecutionRepository;

        public StockSellJob(ILogger<StockSellJob> logger,
            IAccessTokenService accessTokenService,
            IActiveStockRepository activeStockRepository,
            IUpstoxApiClient upstoxApiClient,
            IMemoryCache memoryCache,
            IStockTradeService stockTradeService,
            IJobExecutionRepository jobExecutionRepository)
        {
            this._logger = logger;
            this._accessTokenService = accessTokenService;
            this.activeStockRepository = activeStockRepository;
            this.upstoxApiClient = upstoxApiClient;
            this.memoryCache = memoryCache;
            this.stockTradeService = stockTradeService;
            this.jobExecutionRepository = jobExecutionRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            this._logger.LogInformation("Starting Stock Sell job");
            string JobName = context?.JobDetail?.Key?.Name ?? "Test";
            string jobId = $"{JobName}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

            DateTimeOffset currentDateIndia = DateTimeOffset.UtcNow.ToIndiaTime();
            if (currentDateIndia.DayOfWeek == DayOfWeek.Sunday || currentDateIndia.DayOfWeek == DayOfWeek.Saturday)
            {
                return;
            }
            if (currentDateIndia.Hour < 10 || currentDateIndia.Hour > 16)
            {
                return;
            }
            await this.jobExecutionRepository.StartJobExecution(jobId, JobName);
            try
            {
                List<AccessToken> accessTokenList = await this._accessTokenService.GetAllActiveAccessToken();
                if (accessTokenList == null || accessTokenList.Count == 0)
                {
                    return;
                }
                await SellTask(accessTokenList);
                await this.jobExecutionRepository.JobSucceeded(jobId);
            }
            catch (Exception ex)
            {
                await this.jobExecutionRepository.JobFailed(jobId, ex.Message);
            }
        }

        private async Task SellTask(List<AccessToken> accessTokenList)
        {
            foreach (AccessToken accessToken in accessTokenList)
            {
                List<ActiveStock> activeStocks = await FetchStockAndUpdateCurrentPrice(accessToken);
                this._logger.LogInformation($"Found active stocks {activeStocks.Count}");
                if (activeStocks != null && activeStocks.Count > 0)
                {
                    foreach (ActiveStock activeStock in activeStocks)
                    {
                        if (IsReadyToSell(activeStock))
                        {
                            this._logger.LogInformation($"Stock ready to sell {activeStock.Name}");
                            string orderId = await this.upstoxApiClient.SellStock(activeStock, activeStock.CurrentPrice, accessToken.Accesstoken);
                            activeStock.SellOrderId = orderId;
                            await this.activeStockRepository.UpdateActiveStock(activeStock);
                            this.memoryCache.Set<bool>("SoldStock", true, DateTimeOffset.Now.AddHours(12));
                            this._logger.LogInformation($"Sell Order placed with Order Id {orderId} for stock {activeStock.Name} at price {activeStock.CurrentPrice}");
                            return;
                        }
                        if (!string.IsNullOrEmpty(activeStock.SellOrderId))
                        {
                            OrderDetailResponse detailResponse = await this.upstoxApiClient.GetSellOrderDetails(activeStock.SellOrderId, accessToken.Accesstoken);
                            if (detailResponse != null && detailResponse.Status == "success" && detailResponse.Data.Status == "complete")
                            {
                                await this.stockTradeService.SellStock(activeStock, detailResponse.Data.AveragePrice);
                                this._logger.LogInformation($"Stock {activeStock.Name} Sold Successfully for Price {detailResponse.Data.AveragePrice}");
                            }
                        }
                    }
                }
            }
        }

        private bool IsReadyToSell(ActiveStock stock)
        {
            if (this.memoryCache.TryGetValue<bool>("SoldStock", out bool val) && val == true)
            {
                this._logger.LogInformation($"Stock already sold today");
                return false;
            }
            DateTimeOffset currentDate = DateTimeOffset.UtcNow.ToIndiaTime();
            if (string.IsNullOrEmpty(stock.SellOrderId))
            {
                double gainPercent = (100d * (stock.CurrentPrice - stock.BuyPrice)) / stock.BuyPrice;
                if ((currentDate - stock.BuyTime).Days > 20 && stock.CurrentPrice > stock.BuyPrice)
                {
                    return true;
                }
                if (gainPercent > 15 && stock.CurrentPrice < stock.HighestPrice)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<List<ActiveStock>> FetchStockAndUpdateCurrentPrice(AccessToken accessToken)
        {
            List<ActiveStock> availableActiveStocks = new List<ActiveStock>();
            List<ActiveStock> activeStocks = await this.activeStockRepository.GetAllActiveStocksAsync(accessToken.UserId);
            if (activeStocks == null || activeStocks.Count == 0)
            {
                return availableActiveStocks;
            }
            List<string> stockIds = activeStocks.Select(x => x.StockId).ToList();
            Dictionary<string, double> priceDictionary = await this.upstoxApiClient.GetStockPrice(stockIds, accessToken.Accesstoken);

            foreach (ActiveStock stock in activeStocks)
            {
                if (priceDictionary.ContainsKey(stock.StockId))
                {
                    stock.CurrentPrice = priceDictionary[stock.StockId];
                    double highestPrice = stock.HighestPrice;
                    stock.HighestPrice = Math.Max(stock.CurrentPrice, highestPrice);
                    availableActiveStocks.Add(stock);
                }
            }
            _ = Task.Run(() =>
            {
                this.activeStockRepository.UpdateActiveStocks(availableActiveStocks);
            });
            return availableActiveStocks;
        }
    }
}
