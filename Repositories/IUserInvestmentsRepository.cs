using TheMarauderMap.Entities;

namespace TheMarauderMap.Repositories
{
    public interface IUserInvestmentsRepository
    {
        public Task AddOrUpdateInvestment(double invested, double returnAmount, string userId);

        public Task<UserInvestment> GetUserInvestment(string userId);
    }
}
