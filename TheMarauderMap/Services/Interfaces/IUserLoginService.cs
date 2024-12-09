namespace TheMarauderMap.Services.Interfaces
{
    public interface IUserLoginService
    {
        public Task LoginUser(string code, string userSessionId);

        public Task<bool> IsLoggedIn(string userSessionId);

        public Task<string> GetUserId(string userSessionId);

    }
}
