using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Repository.DTOs.Configuration;
using Pds.DocumentExchange.Data.Repository.Interfaces;

namespace Pds.DocumentExchange.Data.Repository.Implementations
{
    /// <summary>
    /// The documents repository.
    /// </summary>
    public class DocumentsRepository : AzureCosmosDbRepository, IDocumentsRepository
    {
        /// <summary>Initializes a new instance of the <see cref="DocumentsRepository"/> class.</summary>
        /// <param name="cosmosClientFactory">The Cosmos Db client factory.</param>
        /// <param name="config">The Azure Cosmos Db repository configuration.</param>
        /// <param name="logger">The logging service.</param>
        public DocumentsRepository(
            ICosmosClientFactory cosmosClientFactory,
            AzureCosmosDbRepositoryConfiguration config,
            ILoggerAdapter<AzureCosmosDbRepository> logger)
            : base(cosmosClientFactory, config.DatabaseName, config.CollectionName, logger)
        {
        }
    }
}