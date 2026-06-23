using Microsoft.Azure.Cosmos;

namespace Pds.DocumentExchange.Data.Repository.Interfaces
{
    /// <summary>
    /// The Cosmos Db client factory.
    /// </summary>
    public interface ICosmosClientFactory
    {
        /// <summary>
        /// Gets the Cosmos Db client.
        /// </summary>
        /// <returns>The CosmosClient.</returns>
        CosmosClient GetCosmosClient();

        /// <summary>
        /// Gets the Cosmos Db client that allows bulk execution.
        /// </summary>
        /// <returns>The CosmosClient.</returns>
        CosmosClient GetCosmosClientForBulkExecution();
    }
}