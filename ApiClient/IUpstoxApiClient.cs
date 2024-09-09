
using TheMarauderMap.Responses;

namespace TheMarauderMap.ApiClient
{
    public interface IUpstoxApiClient
    {
        public Task<AccessTokenResponse> GenerateAccessToken(string code);
    }
}
