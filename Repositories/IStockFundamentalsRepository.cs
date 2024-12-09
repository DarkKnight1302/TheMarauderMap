using TheMarauderMap.Entities;
using TheMarauderMap.Models;

namespace TheMarauderMap.Repositories
{
    public interface IStockFundamentalsRepository
    {
        public Task<Dictionary<string, StockFundamentals>> GetStockFundamentals(List<string> stockIds);

        public Task<List<string>> GetStaleStocks(int top = 3000);

        public Task UpsertStockFundamentals(StockFundamentalsResp stockFundamentalResp, string stockId, string stockName = null);
    }
}
