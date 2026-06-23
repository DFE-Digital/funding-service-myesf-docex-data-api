using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Pds.DocumentExchange.Data.Populator.Models;
using Pds.DocumentExchange.Data.Repository.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using static Pds.DocumentExchange.Data.Populator.Storage.DocumentsCosmosDb;

namespace Pds.DocumentExchange.Data.Populator.Storage
{
    /// <summary>
    /// The Azure Cosmos Db service.
    /// </summary>
    public abstract class AzureCosmosDb
    {
        protected CosmosClient CosmosClient { get; }

        protected string DatabaseName { get; }

        protected string CollectionName { get; }

        protected PartitionKey PartitionKey { get; } = new PartitionKey(CosmosDbConstants.DefaultPartitionKey);

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCosmosDb"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        protected AzureCosmosDb(Configuration configuration)
        {
            CosmosClient = new CosmosClient(
                configuration.EndPoint,
                configuration.AuthKeyOrResourceToken,
                new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway });
            DatabaseName = configuration.DatabaseName;
            CollectionName = configuration.CollectionName;
        }

        /// <summary>
        /// Deletes all documents.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/> returning the number of deleted documents.</returns>
        public async Task<int> DeleteAllDocuments()
        {
            var cosmosDeletedCount = 0;
            var continuation = true;

            while (continuation)
            {
                StoredProcedureExecuteResponse<BulkDeleteResult> result;

                try
                {
                    result = await ExecuteBulkDelete();
                }
                catch (CosmosException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        await CreateBulkDeleteStoredProcedure();
                        result = await ExecuteBulkDelete();
                    }
                    else
                    {
                        throw;
                    }
                }

                continuation = result.Resource.Continuation;
                cosmosDeletedCount += result.Resource.Deleted;
            }

            return cosmosDeletedCount;
        }

        private Task<StoredProcedureExecuteResponse<BulkDeleteResult>> ExecuteBulkDelete()
        {
            var container = CosmosClient.GetContainer(DatabaseName, CollectionName);

            var partitionKey = new PartitionKey(CosmosDbConstants.DefaultPartitionKey);
            string query = "select c._self from c where NOT IS_DEFINED(c.documentType) OR(c.documentType <> \"Configuration\" AND c.documentType <> \"ListConfiguration\")";

            return container.Scripts.ExecuteStoredProcedureAsync<BulkDeleteResult>("bulkDelete", partitionKey, new dynamic[] { query });
        }

        private async Task CreateBulkDeleteStoredProcedure()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var storedProcedureResource = assembly.GetManifestResourceNames().Single(str => str.EndsWith("bulkDelete.js"));

            var storedProcedureBody = string.Empty;

            using (var fileStream = assembly.GetManifestResourceStream(storedProcedureResource))
            {
                using (var streamReader = new StreamReader(fileStream))
                {
                    storedProcedureBody = streamReader.ReadToEnd();
                }
            }

            var storedProcedureProperties = new StoredProcedureProperties("bulkDelete", storedProcedureBody);

            var container = CosmosClient.GetContainer(DatabaseName, CollectionName);
            await container.Scripts.CreateStoredProcedureAsync(storedProcedureProperties);
        }
    }
}