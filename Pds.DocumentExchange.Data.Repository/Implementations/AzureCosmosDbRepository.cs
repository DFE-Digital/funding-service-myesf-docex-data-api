using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Core.Common;
using Pds.DocumentExchange.Data.Repository.Common;
using Pds.DocumentExchange.Data.Repository.DTOs;
using Pds.DocumentExchange.Data.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Repository.Implementations
{
    /// <summary>The AzureCosmosDbRepository - a wrapper for exposing Azure Cosmos db functionality.</summary>
    /// <seealso cref="IAzureCosmosDbRepository" />
    public class AzureCosmosDbRepository : IAzureCosmosDbRepository
    {
        private readonly ICosmosClientFactory _cosmosClientFactory;
        private readonly ILoggerAdapter<AzureCosmosDbRepository> _logger;

        private readonly Container _container;
        private readonly Container _containerForBulkExecution;

        private readonly PartitionKey _partitionKey = new PartitionKey(CosmosDbConstants.DefaultPartitionKey);

        /// <summary>Initializes a new instance of the <see cref="AzureCosmosDbRepository"/> class.</summary>
        /// <param name="cosmosClientFactory">The Cosmos Db client factory.</param>
        /// <param name="databaseName">The database name.</param>
        /// <param name="collectionName">The collection name.</param>
        /// <param name="logger">The logging service.</param>
        public AzureCosmosDbRepository(
            ICosmosClientFactory cosmosClientFactory,
            string databaseName,
            string collectionName,
            ILoggerAdapter<AzureCosmosDbRepository> logger)
        {
            _cosmosClientFactory = cosmosClientFactory;
            _logger = logger;

            _container = GetContainer(databaseName, collectionName, () => _cosmosClientFactory.GetCosmosClient());
            _containerForBulkExecution = GetContainer(databaseName, collectionName, () => _cosmosClientFactory.GetCosmosClientForBulkExecution());
        }

        /// <inheritdoc/>
        public async Task CreateStoredProcedure(string storedProcedureName, string storedProcedureBody)
        {
            var storedProcedureProperties = new StoredProcedureProperties(storedProcedureName, storedProcedureBody);
            await _container.Scripts.CreateStoredProcedureAsync(storedProcedureProperties);

            _logger.LogInformation($"Created stored procedure {storedProcedureName} in {_container.Id}");
        }

        /// <inheritdoc/>
        public async Task CreateDocument<TDocument>(TDocument document)
            where TDocument : CosmosDbDocument
        {
            await _container.CreateItemAsync(document, new PartitionKey(document.PartitionKey));
        }

        /// <inheritdoc/>
        public async Task UpsertDocument<TDocument>(TDocument document)
            where TDocument : CosmosDbDocument
        {
            await _container.UpsertItemAsync(document, _partitionKey);
        }

        /// <inheritdoc/>
        public async Task<TDocument> GetDocumentById<TDocument>(string id)
            where TDocument : class
        {
            try
            {
                var response = await _container.ReadItemAsync<TDocument>(id, _partitionKey);
                return response.Resource;
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TDocument>> RunQuery<TDocument>(string sqlQuery, params (string Name, object Value)[] parameters)
        {
            var queryDefinition = new QueryDefinition(sqlQuery);

            if (parameters != null && parameters.Any())
            {
                foreach (var currentParameter in parameters)
                {
                    queryDefinition.WithParameter(currentParameter.Name, currentParameter.Value);
                }
            }

            var iterator = _container.GetItemQueryIterator<TDocument>(
                queryDefinition,
                null,
                new QueryRequestOptions
                {
                    PartitionKey = _partitionKey
                });

            var result = new List<TDocument>();

            while (iterator.HasMoreResults)
            {
                var feedResponse = await iterator.ReadNextAsync();
                result.AddRange(feedResponse.AsEnumerable());
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<TValue> RunStoredProcedure<TValue>(string storedProcedureName, dynamic[] storedProcedureParams)
        {
            TValue response;

            try
            {
                var storedProcedureResponse = await _container.Scripts.ExecuteStoredProcedureAsync<TValue>(
                    storedProcedureName,
                    _partitionKey,
                    storedProcedureParams);

                response = storedProcedureResponse.Resource;
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new CosmosDbException { StatusCode = HttpStatusCode.NotFound };
                }

                throw;
            }

            return response;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TValue>> RunStoredProcedureWithContinuationToken<TValue>(string storedProcedureName, dynamic[] storedProcedureParams)
        {
            List<TValue> results = new List<TValue>();

            try
            {
                bool continuation = true;
                dynamic continuationToken = false;

                while (continuation)
                {
                    dynamic[] updatedParams = storedProcedureParams.Concat(new dynamic[] { continuationToken }).ToArray();

                    StoredProcedureExecuteResponse<StoredProcedureWithContinuationTokenResult<TValue>> response = await _container.Scripts.ExecuteStoredProcedureAsync<StoredProcedureWithContinuationTokenResult<TValue>>(
                    storedProcedureName,
                    _partitionKey,
                    updatedParams);

                    results.Add(response.Resource.Documents);
                    continuationToken = response.Resource.LastContinuationToken;
                    continuation = !string.IsNullOrWhiteSpace(response.Resource.LastContinuationToken);
                }
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new CosmosDbException { StatusCode = HttpStatusCode.NotFound };
                }

                throw;
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<string> ReadStoredProcedure(string storedProcedureName)
        {
            string storedProcedureBody;

            try
            {
                var storedProcedureResponse = await _container.Scripts.ReadStoredProcedureAsync(storedProcedureName);

                storedProcedureBody = storedProcedureResponse.Resource.Body;
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new CosmosDbException { StatusCode = HttpStatusCode.NotFound };
                }

                throw;
            }

            return storedProcedureBody;
        }

        /// <inheritdoc />
        public async Task DeleteStoredProcedure(string storedProcedureName)
        {
            await _container.Scripts.DeleteStoredProcedureAsync(storedProcedureName);

            _logger.LogInformation($"Deleted stored procedure {storedProcedureName} in {_container.Id}");
        }

        /// <inheritdoc/>
        public async Task BulkImport<TDocument>(IEnumerable<TDocument> documents)
            where TDocument : CosmosDbDocument
        {
            var tasks = new List<Task>(documents.Count());

            foreach (TDocument currentDocument in documents)
            {
                var createItemTask = _containerForBulkExecution.CreateItemAsync(currentDocument, new PartitionKey(currentDocument.PartitionKey))
                    .ContinueWith(itemResponse =>
                    {
                        if (!itemResponse.IsCompletedSuccessfully)
                        {
                            AggregateException innerExceptions = itemResponse.Exception.Flatten();
                            if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                            {
                                _logger.LogError($"Received {cosmosException.StatusCode} ({cosmosException.Message}).");
                            }
                            else
                            {
                                _logger.LogError($"Exception {innerExceptions.InnerExceptions.FirstOrDefault()}.");
                            }
                        }
                    });

                tasks.Add(createItemTask);
            }

            await Task.WhenAll(tasks);
        }

        private static Container GetContainer(string databaseName, string collectionName, Func<CosmosClient> getCosmosClient)
        {
            var cosmosClient = getCosmosClient();
            return cosmosClient.GetContainer(databaseName, collectionName);
        }
    }
}