using TheMarauderMap.Entities;

namespace TheMarauderMap.Services.Interfaces
{
    public interface IStockScoringService
    {
        public Task<Dictionary<string, int>> GetStockGrowthScores(List<Stock> stocks);
    }
}
