
using Microsoft.Azure.Cosmos;
using TheMarauderMap.Entities;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.Repositories
{
    public class UserBlackListedRepository : IUserBlackListRepository
    {
        private readonly ICosmosDbService cosmosDbService;
        private readonly ILogger<UserBlackListedRepository> logger;
        public UserBlackListedRepository(ICosmosDbService cosmosDbService, ILogger<UserBlackListedRepository> logger)
        {
            this.cosmosDbService = cosmosDbService;
            this.logger = logger;
        }

        public async Task AddStockToBlackList(string userId, string stockId)
        {
            var container = FetchContainer();
            UserBlackListed userBlackListed = await GetUserBlackListedAsync(userId);
            if (userBlackListed == null)
            {
                userBlackListed = new UserBlackListed()
                {
                    UserId = userId,
                    Id = userId
                };
            }
            userBlackListed.BlackListedStocks.Add(stockId);
            await container.UpsertItemAsync(userBlackListed, new PartitionKey(userId));
        }

        public async Task<List<string>> GetBlackListedStocks(string userId)
        {
            UserBlackListed userBlackListed = await GetUserBlackListedAsync(userId);
            if (userBlackListed == null)
            {
                return new List<string>();
            }
            return userBlackListed.BlackListedStocks;
        }

        private async Task<UserBlackListed> GetUserBlackListedAsync(string userId)
        {
            var container = FetchContainer();
            try
            {
                ItemResponse<UserBlackListed> itemResponse = await container.ReadItemAsync<UserBlackListed>(userId, new PartitionKey(userId));
                return itemResponse.Resource;
            }
            catch (CosmosException)
            {
                return null;
            }
        }

        private Container FetchContainer()
        {
            return this.cosmosDbService.GetContainer("UserBlackListed");
        }
    }
}
