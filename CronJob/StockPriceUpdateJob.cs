using Quartz;
using TheMarauderMap.Repositories;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.CronJob
{
    public class StockPriceUpdateJob : IJob
    {
        private readonly IAccessTokenService accessTokenService;
        private readonly ILogger<StockPriceUpdateJob> logger;
        private readonly IStockRepository stockRepository;

        public StockPriceUpdateJob(IAccessTokenService accessTokenService,
            ILogger<StockPriceUpdateJob> logger,
            IStockRepository stockRepository)
        {
            this.accessTokenService = accessTokenService;
            this.logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string JobName = context.JobDetail.Key.Name;
            string accessToken = await this.accessTokenService.GetActiveAccessToken();
            if (accessToken == null)
            {
                this.logger.LogError("Access Token not found");
                return;
            }

            await Task.Delay(100);
        }
    }
}
