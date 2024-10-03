using TheMarauderMap.Services.Interfaces;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TheMarauderMap.Responses;
using TheMarauderMap.Entities;
using System.Text;

namespace TheMarauderMap.ApiClient
{
    public class UpStoxApiClient : IUpstoxApiClient
    {
        private const string BaseUrl = "https://api.upstox.com/v2";
        private readonly ISecretService secretService;
        private readonly string ClientId;
        private readonly string ClientSecret;
        private readonly ILogger logger;
        private readonly string RedirectUri;

        public UpStoxApiClient(ISecretService secretService, ILogger<UpStoxApiClient> logger)
        {
            this.secretService = secretService;
            this.ClientId = secretService.GetSecretValue("UptstoxApiKey");
            this.ClientSecret = secretService.GetSecretValue("UpstoxApiSecret");
            this.RedirectUri = secretService.GetSecretValue("redirect_uri");
            this.logger = logger;
        }
        public async Task<AccessTokenResponse> GenerateAccessToken(string code)
        {
            using (HttpClient client = new HttpClient())
            {
                string tokenUrl = "/login/authorization/token";
                var grantType = "authorization_code";

                // Prepare the request content
                var requestBody = new Dictionary<string, string>
                {
                    { "code", code },
                    { "client_id", ClientId },
                    { "client_secret", ClientSecret },
                    { "redirect_uri", RedirectUri },
                    { "grant_type", grantType }
                };

                var content = new FormUrlEncodedContent(requestBody);

                try
                {
                    // Make the POST request
                    HttpResponseMessage response = await client.PostAsync($"{BaseUrl}{tokenUrl}", content);

                    // Check for success
                    response.EnsureSuccessStatusCode();

                    string responseString = await response.Content.ReadAsStringAsync();

                    // Read the response content
                    AccessTokenResponse accessTokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(responseString);
                    return accessTokenResponse;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Request error: " + e.Message);
                }
                return null;
            }
        }

        public async Task<OrderDetailResponse> GetSellOrderDetails(string orderId, string accessToken)
        {
            string apiUrl = "https://api.upstox.com/v2/order/details";
            using (HttpClient client = new HttpClient())
            {
                // Set the headers
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                // Build the query string with the order_id parameter if it's provided
                string urlWithQuery = string.IsNullOrEmpty(orderId) ? apiUrl : $"{apiUrl}?order_id={orderId}";

                // Make the GET request
                HttpResponseMessage response = await client.GetAsync(urlWithQuery);

                // Ensure the response was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as a string
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    // Deserialize the response into an OrderDetails object
                    var orderDetails = JsonConvert.DeserializeObject<OrderDetailResponse>(jsonResponse);
                    return orderDetails;
                }
                else
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                }
            }
            return null;
        }

        public async Task<Dictionary<string, double>> GetStockPrice(List<string> stockIds, string accessToken)
        {
            // Initialize HttpClient
            Dictionary<string, double> stockPriceDict = new Dictionary<string, double>();
            using (HttpClient client = new HttpClient())
            {
                // Set request headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Api-Version", "2.0");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                string combinedSymbol = string.Join(",", stockIds);
                // Define the request URI
                string requestUri = $"{BaseUrl}/market-quote/ltp?symbol={combinedSymbol}";

                // Send the request
                HttpResponseMessage response = await client.GetAsync(requestUri);

                // Ensure we got a successful response
                response.EnsureSuccessStatusCode();

                // Read and output the response content
                string responseBody = await response.Content.ReadAsStringAsync();
                StockApiResponse stockApiResponse = JsonConvert.DeserializeObject<StockApiResponse>(responseBody);
                var data = stockApiResponse.Data;
                foreach (var kv in data)
                {
                    StockData stockData = kv.Value;
                    stockPriceDict.TryAdd(stockData.InstrumentToken, stockData.LastPrice);
                }
                return stockPriceDict;
            }
        }

        public async Task<string> SellStock(ActiveStock stock, double price, string accessToken)
        {
            this.logger.LogInformation($"Selling stock {stock.StockId} : {stock.Name} for price {price}");
            using (var client = new HttpClient())
            {
                SellOrder order = new SellOrder() 
                {
                     Quantity = stock.Quantity,
                     Product = "D",
                     Validity = "DAY",
                     Price = price,
                     InstrumentToken = stock.StockId,
                     OrderType = "LIMIT",
                     TransactionType = "SELL"
                };

                var jsonData = JsonConvert.SerializeObject(order);

                // Create the HttpContent with JSON data
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // Add necessary headers
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Api-Version", "2.0");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                // Send the POST request
                HttpResponseMessage response = await client.PostAsync("https://api-hft.upstox.com/v2/order/place", content);

                string result = await response.Content.ReadAsStringAsync();
                // Check the response
                if (response.IsSuccessStatusCode)
                {
                    SellOrderDetails sellOrderDetails = JsonConvert.DeserializeObject<SellOrderDetails>(result);
                    this.logger.LogInformation($"Sell Order Id {sellOrderDetails?.Data?.OrderId ?? string.Empty}");
                    return sellOrderDetails?.Data?.OrderId ?? string.Empty;
                }
                else
                {
                    logger.LogError("Error: " + response.StatusCode + $"{result}");
                }
            }
            return string.Empty;
        }
    }
}
