using Microsoft.Azure.Cosmos;
using TheMarauderMap.Entities;
using TheMarauderMap.Extensions;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.Repositories
{
    public class AccessTokenRepository : IAccessTokenRepository
    {
        private readonly ICosmosDbService _cosmosDbService;
        private readonly ILogger<AccessTokenRepository> _logger;
        private readonly IRetryStrategy _retryStrategy;
        public AccessTokenRepository(ICosmosDbService cosmosDbService, ILogger<AccessTokenRepository> logger, IRetryStrategy retryStrategy)
        {
            this._cosmosDbService = cosmosDbService;
            _logger = logger;
            this._retryStrategy = retryStrategy;
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

        public async Task<string> GetActiveAccessToken()
        {
            var container = FetchContainer();
            var currentTime = DateTimeOffset.UtcNow.ToIndiaTime();
            var sixHoursAgo = currentTime.AddHours(-6);

            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.RefreshTime >= @sixHoursAgo")
                .WithParameter("@sixHoursAgo", sixHoursAgo);

            var queryResultSetIterator = container.GetItemQueryIterator<AccessToken>(query);

            if (queryResultSetIterator.HasMoreResults)
            {
                var response = await queryResultSetIterator.ReadNextAsync();
                AccessToken accessToken = response.FirstOrDefault();
                return accessToken?.Accesstoken ?? null;
            }

            return null; // No matching token found
        }

        public async Task<List<AccessToken>> GetAllActiveAccessToken()
        {
            List<AccessToken> accessTokenList = new List<AccessToken>();
            var container = FetchContainer();
            var currentTime = DateTimeOffset.UtcNow.ToIndiaTime();
            var sixHoursAgo = currentTime.AddHours(-6);

            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.RefreshTime >= @sixHoursAgo")
                .WithParameter("@sixHoursAgo", sixHoursAgo);

            var queryResultSetIterator = container.GetItemQueryIterator<AccessToken>(query);

            while (queryResultSetIterator.HasMoreResults)
            {
                var response = await queryResultSetIterator.ReadNextAsync();
                accessTokenList.AddRange(response);
            }

            return accessTokenList; // No matching token found
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

            await this._retryStrategy.ExecuteAsync(() => container.UpsertItemAsync(accessTokenObj));
        }

        private Microsoft.Azure.Cosmos.Container FetchContainer()
        {
            return this._cosmosDbService.GetContainer("AccessToken");
        }
    }
}
