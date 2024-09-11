﻿using TheMarauderMap.Extensions;
using Quartz;
using TheMarauderMap.ApiClient;
using TheMarauderMap.Entities;
using TheMarauderMap.Repositories;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.CronJob
{
    public class StockPriceUpdateJob : IJob
    {
        private readonly IAccessTokenService accessTokenService;
        private readonly ILogger<StockPriceUpdateJob> logger;
        private readonly IStockRepository stockRepository;
        private readonly IUpstoxApiClient upstoxApiClient;
        private readonly IRetryStrategy retryStrategy;

        public StockPriceUpdateJob(IAccessTokenService accessTokenService,
            ILogger<StockPriceUpdateJob> logger,
            IStockRepository stockRepository,
            IUpstoxApiClient upstoxApiClient,
            IRetryStrategy retryStrategy)
        {
            this.accessTokenService = accessTokenService;
            this.logger = logger;
            this.stockRepository = stockRepository;
            this.upstoxApiClient = upstoxApiClient;
            this.retryStrategy = retryStrategy;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await this.retryStrategy.ExecuteAsync(() => UpdateStockPrice(context));
        }

        private async Task UpdateStockPrice(IJobExecutionContext context)
        {
            this.logger.LogInformation("Starting Stock Price Update job");
            string JobName = context?.JobDetail?.Key?.Name ?? "Test";
            string accessToken = await this.accessTokenService.GetActiveAccessToken();
            if (accessToken == null)
            {
                this.logger.LogError("Access Token not found");
                return;
            }
            List<Stock> allStocks = await this.stockRepository.GetAllStocks();
            List<string> stockIds = allStocks.Select(x => x.Id).ToList();
            var stockIdChunks = stockIds.Chunk(500);
            Dictionary<string, double> priceDictionary = new Dictionary<string, double>();
            DateTimeOffset currentDate = DateTimeOffset.UtcNow.ToIndiaTime();
            foreach (var chunk in stockIdChunks)
            {
                Dictionary<string, double> prices = await this.upstoxApiClient.GetStockPrice(chunk.ToList(), accessToken);
                foreach (var pr in prices)
                {
                    priceDictionary.TryAdd(pr.Key, pr.Value);
                }
            }
            foreach (Stock stock in allStocks)
            {
                if (!priceDictionary.ContainsKey(stock.Id))
                {
                    continue;
                }
                int n = stock.PriceHistory.Count;
                if (n > 0 && stock.PriceHistory[n - 1].Date.Date == currentDate.Date)
                {
                    continue;
                }

                StockPrice stockPrice = new StockPrice()
                {
                    Date = currentDate,
                    Price = priceDictionary[stock.Id]
                };
                stock.PriceHistory.Add(stockPrice);
                if (stock.PriceHistory.Count > 5)
                {
                    stock.PriceHistory.RemoveAt(0);
                }
            }
            await this.stockRepository.UpdateStockPrice(allStocks);
        }
    }
}
