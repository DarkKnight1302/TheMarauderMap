using TheMarauderMap.Entities;
using TheMarauderMap.Responses;

namespace TheMarauderMap.Services
{
    public interface IStockTradeService
    {
        public Task<bool> PurchaseStock(string sessionId, Stock stock, int quantity, double price);

        public Task<bool> SellStock(ActiveStock stock, double sellingPrice);

        public Task<PurchasedStock> StockToSell(string sessionId);

        public Task<List<PurchasedStock>> GetAllActiveStocks(string sessionId);

        public Task<List<PurchasedStock>> GetAllInActiveStocks(string sessionId);

        public Task BlackListStock(string sessionId, Stock stock);
    }
}
