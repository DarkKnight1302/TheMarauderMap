
using Microsoft.Azure.Cosmos;
using System.ComponentModel;
using TheMarauderMap.Entities;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.Repositories
{
    public class UserBlackListedRepository : IUserBlackListRepository
    {
        private readonly ICosmosDbService cosmosDbService;
        private readonly ILogger<UserBlackListedRepository> logger;
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        public UserBlackListedRepository(ICosmosDbService cosmosDbService, ILogger<UserBlackListedRepository> logger)
        {
            this.cosmosDbService = cosmosDbService;
            this.logger = logger;
        }

        public async Task AddStockToBlackList(string userId, string stockId)
        {
            await this.semaphoreSlim.WaitAsync();
            try
            {
                var container = FetchContainer();
                bool exists = await StockExists(userId, stockId);
                if (!exists)
                {
                    UserBlackListed userBlackListed = new UserBlackListed
                    {
                        Id = Guid.NewGuid().ToString(),
                        StockId = stockId,
                        UniqueId = $"{userId}_{stockId}",
                        UserId = userId,
                    };
                    await container.CreateItemAsync(userBlackListed);
                }
            }
            finally 
            {
                this.semaphoreSlim.Release();
            }
        }

        public async Task<List<string>> GetBlackListedStocks(string userId)
        {
            var container = FetchContainer();
            var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.UserId = @userId")
            .WithParameter("@userId", userId);

            List<string> results = new List<string>();
            using (var iterator = container.GetItemQueryIterator<UserBlackListed>(query))
            {
                while (iterator.HasMoreResults)
                {
                    foreach (var item in await iterator.ReadNextAsync())
                    {
                        if (item != null && !string.IsNullOrEmpty(item.StockId))
                        {
                            results.Add(item.StockId);
                        }
                    }
                }
            }
            return results;
        }

        private async Task<bool> StockExists(string userId, string stockId)
        {
            var container = FetchContainer();
            var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.UserId = @userId AND c.StockId = @stockId")
            .WithParameter("@userId", userId)
            .WithParameter("@stockId", stockId);
            List<UserBlackListed> results = new List<UserBlackListed>();
            using (var iterator = container.GetItemQueryIterator<UserBlackListed>(query))
            {
                while (iterator.HasMoreResults)
                {
                    foreach (var item in await iterator.ReadNextAsync())
                    {
                        results.Add(item);
                    }
                }
            }
            return results.Any();
        }

        private Microsoft.Azure.Cosmos.Container FetchContainer()
        {
            return this.cosmosDbService.GetContainer("UserBlackListed");
        }
    }
}
