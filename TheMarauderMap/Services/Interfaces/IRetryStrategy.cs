namespace TheMarauderMap.Services.Interfaces
{
    public interface IRetryStrategy
    {
        Task<T> ExecuteAsync<T>(Func<Task<T>> action);

        Task ExecuteAsync(Func<Task> action);
    }
}
