namespace TheMarauderMap.Services.Interfaces
{
    public interface IUserLoginService
    {
        public void SetUserLoginCode(string code, string userId);

        public string GetUserLoginCode(string userId);
    }
}
