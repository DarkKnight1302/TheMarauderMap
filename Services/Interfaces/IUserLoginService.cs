namespace TheMarauderMap.Services.Interfaces
{
    public interface IUserLoginService
    {
        public void SetUserLoginCode(string code);

        public string GetUserLoginCode();
    }
}
