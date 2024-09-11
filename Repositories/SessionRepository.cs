using Microsoft.Azure.Cosmos;
using TheMarauderMap.Entities;
using TheMarauderMap.Extensions;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly ICosmosDbService cosmosDbService;
        private readonly ILogger _logger;
        private readonly IRetryStrategy retryStrategy;

        public SessionRepository(ICosmosDbService cosmosDbService, ILogger<SessionRepository> logger, IRetryStrategy retryStrategy)
        {
            this.cosmosDbService = cosmosDbService;
            this._logger = logger;
            this.retryStrategy = retryStrategy;
        }

        public async Task<string> CreateNewSession()
        {
            var container = FetchContainer();
            string guid = Guid.NewGuid().ToString();
            Session session = new Session()
            {
                Id = guid,
                SessionId = guid,
                CreationTime = DateTimeOffset.UtcNow.ToIndiaTime(),
                ExpiryTime = DateTimeOffset.UtcNow.AddMinutes(15).ToIndiaTime()
            };
            ItemResponse<Session> itemResponse = await container.CreateItemAsync(session, new PartitionKey(guid));
            return itemResponse.Resource.SessionId;
        }

        public async Task<Session> GetSession(string sessionId)
        {
            var container = FetchContainer();
            try
            {
                ItemResponse<Session> itemResponse = await container.ReadItemAsync<Session>(sessionId, new PartitionKey(sessionId));
                return itemResponse.Resource;
            }
            catch (CosmosException)
            {
                return null;
            }
        }

        public async Task UpdateSession(string sessionId, string AccessToken, string userId)
        {
            var container = FetchContainer();
            Session session = await GetSession(sessionId);
            if (session != null)
            {
                session.Accesstoken = AccessToken;
                session.UserId = userId;
                await this.retryStrategy.ExecuteAsync(() => container.ReplaceItemAsync(session, sessionId, new PartitionKey(sessionId)));
            }
        }

        private Container FetchContainer()
        {
            return this.cosmosDbService.GetContainer("Session");
        }
    }
}
