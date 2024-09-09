using TheMarauderMap.Entities;

namespace TheMarauderMap.Repositories
{
    public interface ISessionRepository
    {
        public Task<string> CreateNewSession();

        public Task UpdateSession(string sessionId, string AccessToken, string userId);

        public Task<Session> GetSession(string sessionId);
    }
}
