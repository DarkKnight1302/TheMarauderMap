using CricHeroesAnalytics.Extensions;
using CricHeroesAnalytics.Services.Interfaces;
using Microsoft.Azure.Cosmos;
using TheMarauderMap.Entities;

namespace TheMarauderMap.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly ICosmosDbService cosmosDbService;
        private readonly ILogger _logger;

        public SessionRepository(ICosmosDbService cosmosDbService, ILogger<SessionRepository> logger)
        {
            this.cosmosDbService = cosmosDbService;
            this._logger = logger;
        }
        public async Task<string> CreateNewSession()
        {
            var container = FetchContainer();
            string guid = Guid.NewGuid().ToString();
            Session session = new Session()
            {
                Id = guid,
                SessionId = guid,
                CreationTime = DateTimeOffset.UtcNow.ToIndiaTime()
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
                await container.ReplaceItemAsync(session, sessionId, new PartitionKey(sessionId));
            }
        }

        private Container FetchContainer()
        {
            return this.cosmosDbService.GetContainer("Session");
        }
    }
}
