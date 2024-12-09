using TheMarauderMap.Responses;

namespace TheMarauderMap.Services.Interfaces
{
    public interface IStockRecommendationService
    {
        public Task<List<RecommendedStock>> RecommendStocks(string sessionId);
    }
}
