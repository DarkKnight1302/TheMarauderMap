using TheMarauderMap.Models;

namespace TheMarauderMap.ApiClient
{
    public interface IScreenerClient
    {
        public StockFundamentalsResp GetStockFundamentals(string stockId);
    }
}
