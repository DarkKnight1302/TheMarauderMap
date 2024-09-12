using TheMarauderMap.ApiClient;
using TheMarauderMap.Entities;
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
        private readonly ISessionRepository _sessionRepository;

        public AccessTokenService(ISecretService secretService,
            IAccessTokenRepository accessTokenRepository,
            ILogger<AccessTokenService> logger,
            IUpstoxApiClient upstoxApiClient,
            ISessionRepository sessionRepository) 
        {
            _secretService = secretService;
            _accessTokenRepository = accessTokenRepository;
            this._logger = logger;
            this._upstoxApiClient = upstoxApiClient;
            this._sessionRepository = sessionRepository;
        }

        public async Task<string> FetchAccessToken(string sessionId)
        {
            Session session = await this._sessionRepository.GetSession(sessionId);
            if (session == null) 
            {
                this._logger.LogError("Session not found");
                return string.Empty;
            }
            string userId = session.UserId;
            return await this._accessTokenRepository.GetAccessToken(userId);
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
