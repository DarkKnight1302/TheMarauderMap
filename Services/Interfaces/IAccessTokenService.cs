using TheMarauderMap.Responses;

namespace TheMarauderMap.Services.Interfaces
{
    public interface IAccessTokenService
    {
        public Task<AccessTokenResponse> GenerateAccessToken(string code);

        public string FetchAccessToken(string userId);

        public Task<string> GetActiveAccessToken();
    }
}
