using CricHeroesAnalytics.Extensions;
using TheMarauderMap.Entities;
using TheMarauderMap.Repositories;
using TheMarauderMap.Responses;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.Services
{
    public class UserLoginService : IUserLoginService
    {
        private readonly IAccessTokenService _accessTokenService;
        private readonly ILogger _logger;
        private ISessionRepository _sessionRepository;

        public UserLoginService(IAccessTokenService accessTokenService, ILogger<UserLoginService> logger, ISessionRepository sessionRepository)
        {
            this._accessTokenService = accessTokenService;
            this._logger = logger;
            this._sessionRepository = sessionRepository;
        }

        public async Task<string> GetUserId(string userSessionId)
        {
            Session session = await this._sessionRepository.GetSession(userSessionId);
            if (session != null)
            {
                return session.UserId;
            }
            return null;
        }

        public async Task<bool> IsLoggedIn(string userSessionId)
        {
            Session session  = await this._sessionRepository.GetSession(userSessionId);
            DateTimeOffset currentTime = DateTimeOffset.UtcNow.ToIndiaTime();
            if (session != null)
            {
                Console.WriteLine($"Session {session.ToString()} : {currentTime}");
            }

            if (session != null && session.ExpiryTime > currentTime && !string.IsNullOrEmpty(session.UserId))
            {
                return true;
            }
            return false;
        }

        public async Task LoginUser(string code, string userSessionId)
        {
            AccessTokenResponse accessTokenResponse = await this._accessTokenService.GenerateAccessToken(code);
            if (accessTokenResponse == null)
            {
                this._logger.LogError($"Access token response not found for user session {userSessionId}, code {code}");
                return;
            }
            await this._sessionRepository.UpdateSession(userSessionId, accessTokenResponse.AccessToken, accessTokenResponse.UserId);
        }
    }
}
