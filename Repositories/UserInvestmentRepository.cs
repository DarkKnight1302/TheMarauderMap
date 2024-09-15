using TheMarauderMap.Entities;
using TheMarauderMap.Extensions;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.Repositories
{
    public class UserInvestmentRepository : IUserInvestmentsRepository
    {
        private readonly ICosmosDbService cosmosDbService;
        private readonly ILogger<UserInvestmentRepository> _logger;
        private readonly IRetryStrategy _retryStrategy;

        public UserInvestmentRepository(ICosmosDbService cosmosDbService, ILogger<UserInvestmentRepository> logger, IRetryStrategy retryStrategy)
        {
            this.cosmosDbService = cosmosDbService;
            this._logger = logger;
            this._retryStrategy = retryStrategy;
        }

        public async Task AddOrUpdateInvestment(double invested, double returnAmount, string userId)
        {
            var container = FetchContainer();
            await this._retryStrategy.ExecuteAsync(async () =>
            {
                UserInvestment investment = await GetInvestment(userId);
                if (investment == null)
                {
                    investment = new UserInvestment()
                    {
                        Id = userId,
                        UserId = userId
                    };
                }
                investment.TotalInvestment += invested;
                investment.TotalReturns += returnAmount;
                investment.UpdatedAt = DateTimeOffset.UtcNow.ToIndiaTime();
                await container.UpsertItemAsync(investment, new Microsoft.Azure.Cosmos.PartitionKey(userId));
            });
        }

        private async Task<UserInvestment> GetInvestment(string userId)
        {
            var container = FetchContainer();
            try
            {
                return await container.ReadItemAsync<UserInvestment>(userId, new Microsoft.Azure.Cosmos.PartitionKey(userId));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Task<UserInvestment> GetUserInvestment(string userId)
        {
            return GetInvestment(userId);
        }

        private Microsoft.Azure.Cosmos.Container FetchContainer()
        {
            return this.cosmosDbService.GetContainer("UserInvestment");
        }
    }
}
