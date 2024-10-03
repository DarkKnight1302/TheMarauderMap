using TheMarauderMap.Entities;

namespace TheMarauderMap.Repositories
{
    public interface IAccessTokenRepository
    {
        public Task UpdateAccessToken(string userId, string accessToken);

        public Task<string> GetAccessToken(string userId);

        public Task<string> GetActiveAccessToken();

        public Task<List<AccessToken>> GetAllActiveAccessToken();
    }
}
