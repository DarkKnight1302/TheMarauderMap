using CricHeroesAnalytics.Services.Interfaces;
using System.Diagnostics;
using TheMarauderMap.ApiClient;
using TheMarauderMap.Repositories;
using TheMarauderMap.Responses;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.Services
{
    public class AccessTokenService : IAccessTokenService
    {
        private readonly IAccessTokenRepository _accessTokenRepository;
        private readonly ILogger<AccessTokenService> _logger;
        private readonly ISecretService _secretService;
        private readonly IUpstoxApiClient _upstoxApiClient;

        public AccessTokenService(ISecretService secretService,
            IAccessTokenRepository accessTokenRepository,
            ILogger<AccessTokenService> logger,
            IUpstoxApiClient upstoxApiClient) 
        {
            _secretService = secretService;
            _accessTokenRepository = accessTokenRepository;
            this._logger = logger;
            this._upstoxApiClient = upstoxApiClient;
        }

        public string FetchAccessToken(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<AccessTokenResponse> GenerateAccessToken(string code)
        {
            AccessTokenResponse accessTokenResponse = await this._upstoxApiClient.GenerateAccessToken(code);
            if (accessTokenResponse != null)
            {
                await this._accessTokenRepository.UpdateAccessToken(accessTokenResponse.UserId, accessTokenResponse.AccessToken);
            }
            return accessTokenResponse;
        }

        public async Task<string> GetActiveAccessToken()
        {
            return await this._accessTokenRepository.GetActiveAccessToken();
        }
    }
}
