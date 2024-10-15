using TheMarauderMap.Entities;

namespace TheMarauderMap.Responses
{
    public class PurchasedStock : ActiveStock
    {
        public PurchasedStock(ActiveStock activeStock)
        {
            this.Name = activeStock.Name;
            this.UserId = activeStock.UserId;
            this.StockId = activeStock.StockId;
            this.Uid = activeStock.Uid;
            this.Id = activeStock.Id;
            this.BuyPrice = activeStock.BuyPrice;
            this.SellPrice = activeStock.SellPrice;
            this.BuyTime = activeStock.BuyTime;
            this.SellTime = activeStock.SellTime;
            this.IsActive = activeStock.IsActive;
            this.Quantity = activeStock.Quantity;
            this.TradingSymbol = activeStock.TradingSymbol;
            this.HighestPrice = activeStock.HighestPrice;
        }

        public double CurrentPrice { get; set; }

        public double GainPercent { get; set; }

        public double GainAmount { get; set; }
    }
}
