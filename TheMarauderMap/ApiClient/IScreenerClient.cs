using TheMarauderMap.Models;

namespace TheMarauderMap.ApiClient
{
    public interface IScreenerClient
    {
        public Task<StockFundamentalsResp> GetStockFundamentals(string stockId, string stockName);

        public Task<string> GetCompanyUrl(string stockName);
    }
}
