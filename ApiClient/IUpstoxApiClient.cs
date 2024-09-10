
using TheMarauderMap.Responses;

namespace TheMarauderMap.ApiClient
{
    public interface IUpstoxApiClient
    {
        public Task<AccessTokenResponse> GenerateAccessToken(string code);

        public Task<Dictionary<string, double>> GetStockPrice(List<string> stockIds, string accessToken);
    }
}
