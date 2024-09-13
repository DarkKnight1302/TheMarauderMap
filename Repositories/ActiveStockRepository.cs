using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using TheMarauderMap.Entities;
using TheMarauderMap.Extensions;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.Repositories
{
    public class ActiveStockRepository : IActiveStockRepository
    {
        private readonly ICosmosDbService cosmosDbService;
        private readonly ILogger<ActiveStockRepository> logger;
        private readonly IRetryStrategy retryStrategy;
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public ActiveStockRepository(ICosmosDbService cosmosDbService, ILogger<ActiveStockRepository> logger, IRetryStrategy retryStrategy)
        {
            this.cosmosDbService = cosmosDbService;
            this.logger = logger;
            this.retryStrategy = retryStrategy;
        }

        public async Task BuyStock(string userId, Stock stock, double buyPrice, int quantity)
        {
            await this.semaphoreSlim.WaitAsync();
            try
            {
                await this.retryStrategy.ExecuteAsync(async () =>
                {
                    string id = Guid.NewGuid().ToString();
                    ActiveStock activeStock = new ActiveStock()
                    {
                        Id = id,
                        Uid = id,
                        UserId = userId,
                        Name = stock.Name,
                        TradingSymbol = stock.TradingSymbol,
                        StockId = stock.Id,
                        BuyPrice = buyPrice,
                        Quantity = quantity,
                        BuyTime = DateTimeOffset.UtcNow.ToIndiaTime(),
                        IsActive = true
                    };
                    var container = FetchContainer();
                    await container.CreateItemAsync(activeStock, new PartitionKey(stock.Id));
                });
            }
            finally
            {
                this.semaphoreSlim.Release();
            }
        }

        public async Task<List<ActiveStock>> GetAllActiveStocksAsync(string userId)
        {
            List<ActiveStock> activeStocks = new List<ActiveStock>();
            var container = FetchContainer();
            var q = container.GetItemLinqQueryable<ActiveStock>().Where(x => x.IsActive && x.UserId == userId);
            var iterator = q.ToFeedIterator();
            while (iterator.HasMoreResults)
            {
                FeedResponse<ActiveStock> res = await iterator.ReadNextAsync();
                activeStocks.AddRange(res);
            }
            return activeStocks;
        }

        public async Task<List<ActiveStock>> GetAllInActiveStocksAsync(string userId)
        {
            List<ActiveStock> inActiveStocks = new List<ActiveStock>();
            var container = FetchContainer();
            var q = container.GetItemLinqQueryable<ActiveStock>().Where(x => !x.IsActive && x.UserId == userId);
            var iterator = q.ToFeedIterator();
            while (iterator.HasMoreResults)
            {
                FeedResponse<ActiveStock> res = await iterator.ReadNextAsync();
                inActiveStocks.AddRange(res);
            }
            return inActiveStocks;
        }

        public async Task SellStock(ActiveStock activeStock, double sellPrice)
        {
            await this.retryStrategy.ExecuteAsync(async () =>
            {
                var container = FetchContainer();
                activeStock.SellPrice = sellPrice;
                activeStock.IsActive = false;
                activeStock.SellTime = DateTimeOffset.UtcNow.ToIndiaTime();
                await container.UpsertItemAsync(activeStock, new PartitionKey(activeStock.StockId));
            });
        }

        private Microsoft.Azure.Cosmos.Container FetchContainer()
        {
            return this.cosmosDbService.GetContainer("ActiveStocks");
        }
    }
}
