using TheMarauderMap.Entities;
using TheMarauderMap.Responses;

namespace TheMarauderMap.Services.Interfaces
{
    public interface IAccessTokenService
    {
        public Task<AccessTokenResponse> GenerateAccessToken(string code);

        public Task<string> FetchAccessToken(string sessionId);

        public Task<string> GetActiveAccessToken();

        public Task<List<AccessToken>> GetAllActiveAccessToken();
    }
}
