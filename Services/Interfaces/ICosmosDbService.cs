using Microsoft.Azure.Cosmos;

namespace CricHeroesAnalytics.Services.Interfaces
{
    public interface ICosmosDbService
    {
        public Container GetContainer(string containerName);
    }
}
