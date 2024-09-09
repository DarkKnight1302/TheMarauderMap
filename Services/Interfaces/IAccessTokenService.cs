namespace TheMarauderMap.Services.Interfaces
{
    public interface IAccessTokenService
    {
        public bool GenerateAccessToken(string code, string userId);

        public string FetchAccessToken(string userId);
    }
}
