using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.Services
{
    public class UserLoginService : IUserLoginService
    {
        private string loginCode = "Test";
        private DateTime lastUpdated;

        public string GetUserLoginCode(string userId)
        {
            return loginCode;
        }

        public void SetUserLoginCode(string code, string userId)
        {
            loginCode = code;
            lastUpdated = DateTime.Now;
        }
    }
}
