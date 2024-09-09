using CricHeroesAnalytics.Extensions;
using CricHeroesAnalytics.Services.Interfaces;
using Microsoft.Azure.Cosmos;
using TheMarauderMap.Entities;

namespace TheMarauderMap.Repositories
{
    public class AccessTokenRepository : IAccessTokenRepository
    {
        private readonly ICosmosDbService _cosmosDbService;
        private readonly ILogger<AccessTokenRepository> _logger;
        public AccessTokenRepository(ICosmosDbService cosmosDbService, ILogger<AccessTokenRepository> logger)
        {
            this._cosmosDbService = cosmosDbService;
            _logger = logger;
        }

        public async Task<string> GetAccessToken(string userId)
        {
            var container = FetchContainer();
            try
            {
                ItemResponse<AccessToken> itemResponse = await container.ReadItemAsync<AccessToken>(userId, new PartitionKey(userId));
                return itemResponse?.Resource?.Accesstoken ?? string.Empty;
            }
            catch (CosmosException)
            {
                this._logger.LogInformation($"Access token not found for User Id {userId}");
            }
            return string.Empty;
        }

        public async Task UpdateAccessToken(string userId, string accessToken)
        {
            var container = FetchContainer();
            AccessToken accessTokenObj = new AccessToken()
            {
                Id = userId,
                UserId = userId,
                Accesstoken = accessToken,
                RefreshTime = DateTimeOffset.UtcNow.ToIndiaTime()
            };
            await container.UpsertItemAsync(accessTokenObj);
        }

        private Container FetchContainer()
        {
            return this._cosmosDbService.GetContainer("AccessToken");
        }
    }
}
