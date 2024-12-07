using TheMarauderMap.Models;

namespace TheMarauderMap.ApiClient
{
    public interface IScreenerClient
    {
        public StockFundamentals GetStockFundamentals(string stockId);
    }
}
