using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using TheMarauderMap.Entities;
using TheMarauderMap.Models;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.Repositories
{
    public class StockFundamentalsRepository : IStockFundamentalsRepository
    {
        private readonly ICosmosDbService cosmosDbService;
        private readonly ILogger<StockFundamentalsRepository> logger;
        private readonly IRetryStrategy retryStrategy;

        public StockFundamentalsRepository(ICosmosDbService cosmosDbService, ILogger<StockFundamentalsRepository> logger, IRetryStrategy retryStrategy)
        {
            this.cosmosDbService = cosmosDbService;
            this.logger = logger;
            this.retryStrategy = retryStrategy;
        }

        public async Task<List<string>> GetStaleStocks(int top = 3000)
        {
            List<string> staleStocks = new List<string>();
            DateTimeOffset stockRefreshDate = DateTimeOffset.Now.AddDays(-30);
            var container = FetchContainer();
            var q = container.GetItemLinqQueryable<StockFundamentals>().Where(x => (x.UpdatedAt == default(DateTimeOffset) || x.UpdatedAt < stockRefreshDate));
            var feedIterator = q.ToFeedIterator();
            while (feedIterator.HasMoreResults && staleStocks.Count < top)
            {
                FeedResponse<StockFundamentals> stockFundamentals = await feedIterator.ReadNextAsync().ConfigureAwait(false);
                staleStocks.AddRange(stockFundamentals.Select(x => x.Id));
            }
            return staleStocks;
        }

        public async Task<Dictionary<string, StockFundamentals>> GetStockFundamentals(List<string> stockIds)
        {
            Dictionary<string, StockFundamentals> result = new Dictionary<string, StockFundamentals>();
            var container = FetchContainer();
            IReadOnlyList<(string id, PartitionKey PartitionKey)> stockInput = stockIds.Select(x => (x, new PartitionKey(x))).ToList();
            FeedResponse<StockFundamentals> stockFundamentalFeedResp = await container.ReadManyItemsAsync<StockFundamentals>(stockInput).ConfigureAwait(false);
            foreach (var stock in stockFundamentalFeedResp)
            {
                result.TryAdd(stock.Id, stock);
            }
            return result;
        }

        public async Task UpsertStockFundamentals(StockFundamentalsResp stockFundamentalResp, string stockId, string stockName)
        {
            if (stockFundamentalResp == null || stockFundamentalResp.YearlyRevenue == null)
            {
                this.logger.LogError("Empty or null data for Stock fundamentals resp");
                return;
            }

            StockFundamentals stockFundamentals = new StockFundamentals()
            {
                Id = stockId,
                Name = stockName,
                Uid = stockId,
                QuarterlyProfit = stockFundamentalResp.QuarterlyProfit,
                QuarterlyRevenue = stockFundamentalResp.QuarterlyRevenue,
                YearlyProfit = stockFundamentalResp.YearlyProfit,
                YearlyRevenue = stockFundamentalResp.YearlyRevenue,
                UpdatedAt = DateTimeOffset.Now
            };

            await this.retryStrategy.ExecuteAsync(async () =>
            {
                var container = FetchContainer();
                await container.UpsertItemAsync<StockFundamentals>(stockFundamentals).ConfigureAwait(false);
            });
        }

        private Container FetchContainer()
        {
            return this.cosmosDbService.GetContainer("StockFundamentals");
        }
    }
}
