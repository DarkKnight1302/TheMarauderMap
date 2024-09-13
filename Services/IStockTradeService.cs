using TheMarauderMap.Entities;
using TheMarauderMap.Responses;

namespace TheMarauderMap.Services
{
    public interface IStockTradeService
    {
        public Task<bool> PurchaseStock(string sessionId, Stock stock, int quantity, double price);

        public Task SellStock(ActiveStock stock, double sellingPrice);

        public Task<PurchasedStock> StockToSell(string sessionId);
    }
}
