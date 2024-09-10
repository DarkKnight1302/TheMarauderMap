using CricHeroesAnalytics.Services.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using TheMarauderMap.Entities;

namespace TheMarauderMap.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly ICosmosDbService cosmosDbService;
        private readonly ILogger _logger;

        public StockRepository(ICosmosDbService cosmosDbService, ILogger<StockRepository> logger) 
        {
            _logger = logger;
            this.cosmosDbService = cosmosDbService;
        }
        public async Task<List<Stock>> GetAllStocks()
        {
            List<Stock> allStocks = new List<Stock>();
            var container = FetchContainer();
            var q = container.GetItemLinqQueryable<Stock>();
            var iterator = q.ToFeedIterator();
            while (iterator.HasMoreResults)
            {
                FeedResponse<Stock> resp = await iterator.ReadNextAsync();
                allStocks.AddRange(resp);
            }
            return allStocks;
        }

        public async Task<bool> StockExists(string stockId)
        {
            var container = FetchContainer();
            try
            {
                ItemResponse<Stock> itemResponse = await container.ReadItemAsync<Stock>(stockId, new PartitionKey(stockId));
                return itemResponse.Resource != null;
            }
            catch (CosmosException)
            {
                return false;
            }
        }

        public async Task UpdateStockPrice(List<Stock> stocks)
        {
            var container = FetchContainer();
            foreach (var stock in stocks)
            {
               await container.UpsertItemAsync(stock, new PartitionKey(stock.Id));
            }
        }

        private Container FetchContainer()
        {
            return this.cosmosDbService.GetContainer("Stock");
        }
    }
}
    