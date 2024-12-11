using HtmlAgilityPack;
using TheMarauderMap.Models;
using TheMarauderMap.Responses;
using TheMarauderMap.Utilities;

namespace TheMarauderMap.ApiClient
{
    public class ScreenerClient : IScreenerClient
    {
        private readonly ILogger<ScreenerClient> logger;
        private readonly HttpClient httpClient;

        public ScreenerClient(ILogger<ScreenerClient> logger)
        {
            this.logger = logger;
            this.httpClient = new HttpClient();
        }

        public async Task<string> GetCompanyUrl(string stockName)
        {
            HttpResponseMessage httpResponse = await httpClient.GetAsync($"https://www.screener.in/api/company/search/?q={stockName}&v=3&fts=1");
            if (httpResponse.IsSuccessStatusCode)
            {
                string responseBody = await httpResponse.Content.ReadAsStringAsync();
                List<ScreenerSearchResponse> searchResponses = JsonUtil.DeSerialize<List<ScreenerSearchResponse>>(responseBody);
                if (searchResponses != null && searchResponses.Count > 1)
                {
                    return searchResponses[0].Url;
                }
            }
            return string.Empty;
        }

        public async Task<StockFundamentalsResp> GetStockFundamentals(string stockId, string stockName)
        {
            StockFundamentalsResp stockFundamentals = new StockFundamentalsResp();
            try
            {
                this.logger.LogInformation($"Fetching Stock fundamentals for {stockId} : {stockName}");
                string companyUrl = await this.GetCompanyUrl(FixStockNameForSplit(stockId));
                this.logger.LogInformation($"Fetching Stock fundamentals for Company {companyUrl}");
                if (string.IsNullOrEmpty(companyUrl))
                {
                    companyUrl = await this.GetCompanyUrl(FixStockNameForDot(stockName));
                    if (string.IsNullOrEmpty(companyUrl))
                    {
                        this.logger.LogError($"Company name not found for stock {stockName}");
                        return null;
                    }
                }
                var Webget = new HtmlWeb();
                var htmlDocument = Webget.Load($"https://www.screener.in/{companyUrl}");
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

        private string FixStockNameForSplit(string stockName)
        {
            if (stockName.Contains('-'))
            {
                string[] splits = stockName.Split('-');
                return splits[0];
            }
            return stockName;
        }

        private string FixStockNameForDot(string stockName)
        {
            if (stockName.EndsWith('.'))
            {
                return stockName.Substring(0, stockName.Length - 1);
            }
            return stockName;
        }
    }
}
