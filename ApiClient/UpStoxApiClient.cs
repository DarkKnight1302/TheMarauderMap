
using CricHeroesAnalytics.Services.Interfaces;
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

                    // Read the response content
                    AccessTokenResponse accessTokenResponse = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
                    return accessTokenResponse;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Request error: " + e.Message);
                }
                return null;
            }
        }
    }
}
