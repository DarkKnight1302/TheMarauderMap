﻿using HtmlAgilityPack;
using TheMarauderMap.Models;
using TheMarauderMap.Utilities;

namespace TheMarauderMap.ApiClient
{
    public class ScreenerClient : IScreenerClient
    {
        private readonly ILogger<ScreenerClient> logger;

        public ScreenerClient(ILogger<ScreenerClient> logger)
        {
            this.logger = logger;
        }

        public StockFundamentalsResp GetStockFundamentals(string stockId)
        {
            StockFundamentalsResp stockFundamentals = new StockFundamentalsResp();
            try
            {
                this.logger.LogInformation($"Fetching Stock fundamentals for {stockId}");
                var Webget = new HtmlWeb();
                var htmlDocument = Webget.Load($"https://www.screener.in/company/{stockId}/");
                if (!HtmlParser.IsValidStockDoc(htmlDocument))
                {
                    return null;
                }
                List<int> yearlyRevenue = HtmlParser.ExtractSectionForRevenue(htmlDocument, "profit-loss");
                List<int> quarterlyRevenue = HtmlParser.ExtractSectionForRevenue(htmlDocument, "quarters");
                List<int> yearlyProfit = HtmlParser.ExtractSectionForProfit(htmlDocument, "profit-loss");
                List<int> quarterlyProfit = HtmlParser.ExtractSectionForProfit(htmlDocument, "quarters");

                if (yearlyRevenue == null || yearlyRevenue.Count == 0 || yearlyProfit == null || yearlyProfit.Count == 0)
                {
                    return null;
                }
                // Discard last values, as they represent TTM
                yearlyRevenue.RemoveAt(yearlyRevenue.Count - 1);
                yearlyProfit.RemoveAt(yearlyProfit.Count - 1);

                stockFundamentals.YearlyRevenue = yearlyRevenue;
                stockFundamentals.YearlyProfit = yearlyProfit;
                stockFundamentals.QuarterlyRevenue = quarterlyRevenue;
                stockFundamentals.QuarterlyProfit = quarterlyProfit;
                return stockFundamentals;
            } catch (Exception e)
            {
                this.logger.LogError($"Exception in ScreenerClient {e.Message} \n {e.StackTrace}");
                throw;
            }
        }
    }
}
