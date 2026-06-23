using Microsoft.Azure.Cosmos;
using Pds.DocumentExchange.Data.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator.Storage
{
    /// <summary>
    /// The populator documents Cosmos Db service.
    /// </summary>
    public class DocumentsCosmosDb : AzureCosmosDb, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentsCosmosDb"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public DocumentsCosmosDb(Configuration configuration)
            : base(configuration)
        {
        }

        /// <inheritdoc/>
        public void Dispose()
            => CosmosClient?.Dispose();

        /// <summary>
        /// Creates a document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task CreateDocument(object document)
        {
            var container = CosmosClient.GetContainer(DatabaseName, CollectionName);
            await container.CreateItemAsync(document, PartitionKey);
        }

        /// <summary>
        /// Gets a document by its identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <typeparam name="TDocument">The Cosmos DB document type.</typeparam>
        /// <returns>An awaitable <see cref="Task"/> returning the document.</returns>
        public async Task<TDocument> GetDocumentById<TDocument>(string id)
        {
            var container = CosmosClient.GetContainer(DatabaseName, CollectionName);
            var response = await container.ReadItemAsync<TDocument>(id, PartitionKey);

            return response.Resource;
        }

        /// <summary>
        /// Gets an entity by parent ID.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="parentBatchIdentifier">The parent batch identifier.</param>
        /// <returns>The collection of entities.</returns>
        public async Task<List<T>> GetEntityByParentId<T>(string parentBatchIdentifier)
        {
            var container = CosmosClient.GetContainer(DatabaseName, CollectionName);

            var queryDefinition = new QueryDefinition("SELECT * FROM Batches WHERE (Batches.ParentBatchIdentifier = @id)");
            queryDefinition.WithParameter("@id", parentBatchIdentifier);

            var iterator = container.GetItemQueryIterator<T>(
                queryDefinition,
                null,
                new QueryRequestOptions
                {
                    PartitionKey = PartitionKey
                });

            var result = new List<T>();

            while (iterator.HasMoreResults)
            {
                var feedResponse = await iterator.ReadNextAsync();
                result.AddRange(feedResponse.AsEnumerable());
            }

            return result;
        }

        /// <summary>
        /// Gets the file metadata by file name.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The file metadata collection.</returns>
        public async Task<List<FileMetadata>> GetFileMetadataByFileName(string fileName)
        {
            var container = CosmosClient.GetContainer(DatabaseName, CollectionName);

            var queryDefinition = new QueryDefinition("SELECT VALUE f FROM c JOIN f IN c.Files WHERE f.Filename = @fileName");
            queryDefinition.WithParameter("@fileName", fileName);

            var iterator = container.GetItemQueryIterator<FileMetadata>(
                queryDefinition,
                null,
                new QueryRequestOptions
                {
                    PartitionKey = PartitionKey
                });

            var result = new List<FileMetadata>();

            while (iterator.HasMoreResults)
            {
                var feedResponse = await iterator.ReadNextAsync();
                result.AddRange(feedResponse.AsEnumerable());
            }

            return result;
        }

        public class Configuration
        {
            public string EndPoint { get; set; }

            public string AuthKeyOrResourceToken { get; set; }

            public string DatabaseName { get; set; }

            public string CollectionName { get; set; }
        }
    }
}