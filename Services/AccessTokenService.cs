using CricHeroesAnalytics.Services.Interfaces;
using System.Diagnostics;
using TheMarauderMap.Repositories;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.Services
{
    public class AccessTokenService : IAccessTokenService
    {
        private readonly IAccessTokenRepository _accessTokenRepository;
        private readonly ILogger<AccessTokenService> _logger;
        private readonly ISecretService _secretService;

        public AccessTokenService(ISecretService secretService, IAccessTokenRepository accessTokenRepository, ILogger<AccessTokenService> logger) 
        {
            _secretService = secretService;
            _accessTokenRepository = accessTokenRepository;
            this._logger = logger;
        }

        public string FetchAccessToken(string userId)
        {
            throw new NotImplementedException();
        }

        public bool GenerateAccessToken(string code, string userId)
        {
            return false;
        }
    }
}
