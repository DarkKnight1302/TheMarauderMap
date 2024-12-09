using Microsoft.Azure.Cosmos;

namespace TheMarauderMap.Services.Interfaces
{
    public interface ICosmosDbService
    {
        public Container GetContainer(string containerName);
    }
}
