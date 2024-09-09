namespace TheMarauderMap.Services.Interfaces
{
    public class UserLoginService : IUserLoginService
    {
        private string loginCode = "Test";
        private DateTime lastUpdated;

        public string GetUserLoginCode()
        {
            return this.loginCode;
        }

        public void SetUserLoginCode(string code)
        {
            loginCode = code;
            this.lastUpdated = DateTime.Now;
        }
    }
}
