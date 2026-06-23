using Microsoft.Azure.Cosmos;
using Pds.DocumentExchange.Data.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pds.DocumentExchange.Data.Repository.Implementations
{
    /// <summary>
    /// The Cosmos Db client implementation.
    /// </summary>
    public class CosmosClientFactory : ICosmosClientFactory
    {
        private readonly IEnumerable<CosmosClient> _cosmosClients;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosClientFactory"/> class.
        /// </summary>
        /// <param name="cosmosClients">The registered Cosmos Db clients.</param>
        public CosmosClientFactory(IEnumerable<CosmosClient> cosmosClients)
        {
            if (cosmosClients == null)
            {
                throw new ArgumentNullException(nameof(cosmosClients), "The cosmos clients collection cannot be null.");
            }

            if (!cosmosClients.Any())
            {
                throw new ArgumentException("The cosmos clients collection cannot be empty.", nameof(cosmosClients));
            }

            _cosmosClients = cosmosClients;
        }

        /// <inheritdoc/>
        public CosmosClient GetCosmosClient()
            => GetCosmosClient(false);

        /// <inheritdoc/>
        public CosmosClient GetCosmosClientForBulkExecution()
            => GetCosmosClient(true);

        private CosmosClient GetCosmosClient(bool allowBulkExecution)
            => _cosmosClients.SingleOrDefault(client => client.ClientOptions.AllowBulkExecution == allowBulkExecution);
    }
}