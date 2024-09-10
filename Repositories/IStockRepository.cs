using TheMarauderMap.Entities;

namespace TheMarauderMap.Repositories
{
    public interface IStockRepository
    {
        public Task UpdateStockPrice(List<Stock> stocks);

        public Task<List<Stock>> GetAllStocks();

        public Task<bool> StockExists(string stockId);

    }
}
