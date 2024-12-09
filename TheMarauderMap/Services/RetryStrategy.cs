using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.Services
{
    public class RetryStrategy : IRetryStrategy
    {
        private readonly int _maxRetries;
        private readonly TimeSpan _delay;

        public RetryStrategy(int maxRetries, TimeSpan delay)
        {
            _maxRetries = maxRetries;
            _delay = delay;
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (retryCount < _maxRetries)
                {
                    retryCount++;
                    Console.WriteLine($"Exception caught: {ex.Message}. Retrying {retryCount}/{_maxRetries}...");
                    await Task.Delay(_delay);
                }
            }
        }

        public async Task ExecuteAsync(Func<Task> action)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    await action();
                    return;
                }
                catch (Exception ex) when (retryCount < _maxRetries)
                {
                    retryCount++;
                    Console.WriteLine($"Exception caught: {ex.Message}. Retrying {retryCount}/{_maxRetries}...");
                    await Task.Delay(_delay);
                }
            }
        }
    }
}
