using TheMarauderMap.Entities;

namespace TheMarauderMap.Repositories
{
    public interface IActiveStockRepository
    {
        public Task BuyStock(string userId, Stock stock, double buyPrice, int quantity);

        public Task SellStock(ActiveStock activeStock, double sellPrice);

        public Task<List<ActiveStock>> GetAllActiveStocksAsync(string userId);

        public Task<List<ActiveStock>> GetAllInActiveStocksAsync(string userId);

        public Task<bool> ActiveStockExists(string id, string stockId);
    }
}
