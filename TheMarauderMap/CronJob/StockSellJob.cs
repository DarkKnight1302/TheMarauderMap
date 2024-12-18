using Azure.Core;
using Microsoft.Extensions.Caching.Memory;
using Quartz;
using System.Text;
using TheMarauderMap.ApiClient;
using TheMarauderMap.Entities;
using TheMarauderMap.Extensions;
using TheMarauderMap.Repositories;
using TheMarauderMap.Responses;
using TheMarauderMap.Services;
using TheMarauderMap.Services.Interfaces;
using AccessToken = TheMarauderMap.Entities.AccessToken;

namespace TheMarauderMap.CronJob
{
    public class StockSellJob : IJob
    {
        private readonly ILogger<StockSellJob> _logger;
        private readonly IAccessTokenService _accessTokenService;
        private readonly IActiveStockRepository activeStockRepository;
        private readonly IUpstoxApiClient upstoxApiClient;
        private readonly IStockTradeService stockTradeService;
        private readonly IJobExecutionRepository jobExecutionRepository;

        public StockSellJob(ILogger<StockSellJob> logger,
            IAccessTokenService accessTokenService,
            IActiveStockRepository activeStockRepository,
            IUpstoxApiClient upstoxApiClient,
            IStockTradeService stockTradeService,
            IJobExecutionRepository jobExecutionRepository)
        {
            this._logger = logger;
            this._accessTokenService = accessTokenService;
            this.activeStockRepository = activeStockRepository;
            this.upstoxApiClient = upstoxApiClient;
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
                string execution = await SellTask(accessTokenList);
                this._logger.LogInformation(execution);
                await this.jobExecutionRepository.JobSucceeded(jobId, execution);
            }
            catch (Exception ex)
            {
                await this.jobExecutionRepository.JobFailed(jobId, ex.Message);
            }
        }

        private async Task<string> SellTask(List<AccessToken> accessTokenList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (AccessToken accessToken in accessTokenList)
            {
                sb.AppendLine($"Start sell task for access token {accessToken.UserId}");
                List<ActiveStock> activeStocks = await FetchStockAndUpdateCurrentPrice(accessToken);
                sb.AppendLine($"Fetching active stocks for user {activeStocks?.Count ?? 0}");
                this._logger.LogInformation($"Found active stocks {activeStocks?.Count ?? 0}");
                if (activeStocks != null && activeStocks.Count > 0)
                {
                    foreach (ActiveStock activeStock in activeStocks)
                    {
                        if (IsReadyToSell(activeStock, sb))
                        {
                            this._logger.LogInformation($"Stock ready to sell {activeStock.Name}");
                            sb.Append($"Trying to sell {activeStock.Name}");
                            string orderId = await this.upstoxApiClient.SellStock(activeStock, activeStock.CurrentPrice, accessToken.Accesstoken);
                            activeStock.SellOrderId = orderId;
                            sb.Append($"Stock sell order id {orderId}");
                            await this.activeStockRepository.UpdateActiveStock(activeStock);
                            sb.Append($"Updating stock in database");
                            this._logger.LogInformation($"Sell Order placed with Order Id {orderId} for stock {activeStock.Name} at price {activeStock.CurrentPrice}");
                            return sb.ToString();
                        }
                        if (!string.IsNullOrEmpty(activeStock.SellOrderId))
                        {
                            OrderDetailResponse detailResponse = await this.upstoxApiClient.GetSellOrderDetails(activeStock.SellOrderId, accessToken.Accesstoken);
                            sb.AppendLine($"Getting order sell details {detailResponse?.Data?.OrderId}");
                            if (detailResponse != null && detailResponse.Status == "success" && detailResponse.Data.Status == "complete")
                            {
                                await this.stockTradeService.SellStock(activeStock, detailResponse.Data.AveragePrice);
                                this._logger.LogInformation($"Stock {activeStock.Name} Sold Successfully for Price {detailResponse.Data.AveragePrice}");
                            }
                        }
                    }
                }
            }
            return sb.ToString();
        }

        private bool IsReadyToSell(ActiveStock stock, StringBuilder sb)
        {
            sb.AppendLine($"Checking ready to sell for stock {stock.Name}");
            DateTimeOffset currentDate = DateTimeOffset.UtcNow.ToIndiaTime();
            sb.AppendLine($"Current date for selling stock {currentDate}");
            if (string.IsNullOrEmpty(stock.SellOrderId))
            {
                sb.AppendLine($"Empty sell Order Id");
                double gainPercent = (100d * (stock.CurrentPrice - stock.BuyPrice)) / stock.BuyPrice;
                sb.AppendLine($"Gain Percent of stock  {gainPercent}");
                sb.AppendLine($"Stock details : Buy time  - {stock.BuyTime} : Current price - {stock.CurrentPrice} : Buy price - {stock.BuyPrice} : Highest price {stock.HighestPrice}");
                if (gainPercent > 15 && stock.CurrentPrice < stock.HighestPrice)
                {
                    sb.AppendLine($"stock gain greater than 15 percent and current price more than buy price");
                    return true;
                }
                if (gainPercent >= 8 && GetDownPercent(stock.CurrentPrice, stock.HighestPrice, stock.BuyPrice) >= 1d)
                {
                    sb.AppendLine($"stock gain greater than 8 percent and current price less than highest price");
                    return true;
                }
                if ((currentDate - stock.BuyTime).Days > 60 && stock.CurrentPrice > stock.BuyPrice)
                {
                    return true;
                }
            }
            return false;
        }

        private double GetDownPercent(double currentPrice, double highestPrice, double basePrice)
        {
            double percent = (100d * (highestPrice - currentPrice)) / basePrice;
            return percent;
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
