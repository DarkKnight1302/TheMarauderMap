
using CricHeroesAnalytics.Services.Interfaces;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TheMarauderMap.Responses;

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

                try
                {
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
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }
            return stockPriceDict;
        }
    }
}
