using CricHeroesAnalytics.Services.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;

namespace CricHeroesAnalytics.Services
{
    public class CosmosDbService : ICosmosDbService
    {
        private const string databaseName = "TheMarauderMap";
        private readonly CosmosClient cosmosClient;
        private readonly IMemoryCache _cache;

        public CosmosDbService(IMemoryCache memoryCache, ISecretService secretService) 
        {
            _cache = memoryCache;
            string connectionString = secretService.GetSecretValue("cosmosConnectionString");
            this.cosmosClient = new CosmosClient(connectionString);
        }
        public Container GetContainer(string containerName)
        {
            if (_cache.TryGetValue(containerName, out Container container))
            {
                return container;
            }
            Database database = cosmosClient.GetDatabase(databaseName);
            Container createdContainer = database.GetContainer(containerName);
            this._cache.Set(containerName, createdContainer, DateTimeOffset.Now.AddHours(2d));
            return createdContainer;
        }
    }
}
