
namespace TheMarauderMap.Repositories
{
    public interface IJobExecutionRepository
    {
        public Task StartJobExecution(string jobId, string JobName);

        public Task JobSucceeded(string jobId, string execution = null);

        public Task JobFailed(string jobId, string reason);

        public Task<DateTimeOffset> GetLastSuccessJobTime(string jobName);
    }
}
