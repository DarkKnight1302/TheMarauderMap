namespace TheMarauderMap.Repositories
{
    public interface IUserBlackListRepository
    {
        public Task AddStockToBlackList(string userId, string stockId);

        public Task<List<string>> GetBlackListedStocks(string userId);
    }
}
