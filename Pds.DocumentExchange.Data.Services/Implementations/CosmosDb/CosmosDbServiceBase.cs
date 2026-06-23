using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Core.Common;
using Pds.DocumentExchange.Data.Repository.DTOs;
using Pds.DocumentExchange.Data.Repository.Interfaces;
using Pds.DocumentExchange.Data.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.CosmosDb
{
    /// <summary>The cosmos db base service.</summary>
    public class CosmosDbServiceBase
    {
        private readonly IAzureCosmosDbRepository _cosmosDbRepository;
        private readonly IFileRepository _fileRepository;
        private readonly ILoggerAdapter<CosmosDbServiceBase> _logger;

        /// <summary>Initializes a new instance of the <see cref="CosmosDbServiceBase"/> class.</summary>
        /// <param name="cosmosDbRepository">The cosmos database repository.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="logger">The logging service.</param>
        public CosmosDbServiceBase(
            IAzureCosmosDbRepository cosmosDbRepository,
            IFileRepository fileRepository,
            ILoggerAdapter<CosmosDbServiceBase> logger)
        {
            _cosmosDbRepository = cosmosDbRepository;
            _fileRepository = fileRepository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new document.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="document">The document.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task CreateDocument<TDocument>(TDocument document)
            where TDocument : CosmosDbDocument
        {
            var performanceTimer = Stopwatch.StartNew();

            try
            {
                await _cosmosDbRepository.CreateDocument(document);

                performanceTimer.Stop();
                _logger.LogInformation(
                    $"{nameof(CosmosDbServiceBase)}.{nameof(CreateDocument)} {document.GetType()} " +
                    $"took {performanceTimer.ElapsedMilliseconds:n0}ms");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(CosmosDbServiceBase)}.{nameof(CreateDocument)} {document.GetType()} failed.");
                throw;
            }
        }

        /// <summary>
        /// Upserts a document to the database.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="document">The document.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task UpsertDocument<TDocument>(TDocument document)
           where TDocument : CosmosDbDocument
        {
            var performanceTimer = Stopwatch.StartNew();

            try
            {
                await _cosmosDbRepository.UpsertDocument(document);

                performanceTimer.Stop();
                _logger.LogInformation(
                    $"{nameof(CosmosDbServiceBase)}.{nameof(UpsertDocument)} {document.GetType()} " +
                    $"took {performanceTimer.ElapsedMilliseconds:n0}ms");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(CosmosDbServiceBase)}.{nameof(UpsertDocument)} {document.GetType()} failed.");
                throw;
            }
        }

        /// <summary>
        /// Gets a document by ID.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="id">The document ID.</param>
        /// <returns>A <see cref="Task"/> returning the document.</returns>
        protected async Task<TDocument> GetDocumentById<TDocument>(string id)
            where TDocument : class
        {
            try
            {
                var performanceTimer = Stopwatch.StartNew();

                var document = await _cosmosDbRepository.GetDocumentById<TDocument>(id);
                performanceTimer.Stop();

                _logger.LogInformation(
                    $"{nameof(CosmosDbServiceBase)}.{nameof(GetDocumentById)} {typeof(TDocument).Name} " +
                    $"took {performanceTimer.ElapsedMilliseconds:n0}ms");

                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(CosmosDbServiceBase)}.{nameof(GetDocumentById)} {typeof(TDocument).Name} failed.");
                throw;
            }
        }

        /// <summary>
        /// Performs a bulk insert of the collection of documents.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents collection.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task BulkImport<TDocument>(IEnumerable<TDocument> documents)
            where TDocument : CosmosDbDocument
        {
            try
            {
                var performanceTimer = Stopwatch.StartNew();

                await _cosmosDbRepository.BulkImport(documents);
                performanceTimer.Stop();

                _logger.LogInformation(
                    $"{nameof(CosmosDbServiceBase)}.{nameof(BulkImport)} {typeof(TDocument).Name} " +
                    $"took {performanceTimer.ElapsedMilliseconds:n0}ms");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(CosmosDbServiceBase)}.{nameof(BulkImport)} {typeof(TDocument).Name} failed.");
                throw;
            }
        }

        /// <summary>Runs the stored procedure.</summary>
        /// <typeparam name="TDocument">The type of the value.</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="storedProcedureParams">The stored procedure parameters.</param>
        /// <returns>The result of the stored procedure.</returns>
        protected async Task<TDocument> RunStoredProcedureAsync<TDocument>(string storedProcedureName, params dynamic[] storedProcedureParams)
        {
            var performanceTimer = Stopwatch.StartNew();

#pragma warning disable S1854 // Unused assignments should be removed
            TDocument result = default;
#pragma warning restore S1854 // Unused assignments should be removed

            try
            {
                result = await _cosmosDbRepository.RunStoredProcedure<TDocument>(storedProcedureName, storedProcedureParams);

                _logger.LogInformation(
                    GetPerformanceLogMessage(storedProcedureName, storedProcedureParams, performanceTimer, false));
            }
            catch (CosmosDbException cosmosDbException)
            {
                _logger.LogError(cosmosDbException, $"{nameof(CosmosDbServiceBase)}.{nameof(RunStoredProcedureAsync)} {storedProcedureName} failed.");

                if (cosmosDbException.StatusCode == HttpStatusCode.NotFound)
                {
                    await CreateStoredProcedure(storedProcedureName);
                    result = await _cosmosDbRepository.RunStoredProcedure<TDocument>(storedProcedureName, storedProcedureParams);

                    _logger.LogInformation(
                        GetPerformanceLogMessage(storedProcedureName, storedProcedureParams, performanceTimer, true));
                }
            }

            return result;

            string GetPerformanceLogMessage(
                string storedProcedureName,
                dynamic[] storedProcedureParams,
                Stopwatch performanceTimer,
                bool storedProcedureCreated)
            {
                performanceTimer.Stop();

                var logMessageBuilder = new StringBuilder();

                logMessageBuilder.AppendLine($"{nameof(CosmosDbServiceBase)}.{nameof(RunStoredProcedureAsync)}");
                logMessageBuilder.AppendLine($"Stored procedure name: {storedProcedureName}");
                logMessageBuilder.AppendLine($"Took: {performanceTimer.ElapsedMilliseconds:n0}ms");

                if (storedProcedureCreated)
                {
                    logMessageBuilder.AppendLine("The stored procedure was created prior to execution");
                }

                if (storedProcedureParams.Length > 0)
                {
                    logMessageBuilder.AppendLine("Params:");

                    foreach (var param in storedProcedureParams)
                    {
                        logMessageBuilder.AppendLine(param.ToString());
                    }
                }

                var logMessage = logMessageBuilder.ToString();
                return logMessage;
            }
        }

        /// <summary>Runs the stored procedure with continuation token.</summary>
        /// <typeparam name="TDocument">The type of the value.</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="storedProcedureParams">The stored procedure parameters.</param>
        /// <returns>The result of the stored procedure.</returns>
        protected async Task<IEnumerable<TDocument>> RunStoredProcedureWithContinuationTokenAsync<TDocument>(string storedProcedureName, params dynamic[] storedProcedureParams)
        {
            var performanceTimer = Stopwatch.StartNew();

#pragma warning disable S1854 // Unused assignments should be removed
            IEnumerable<TDocument> results = default;
#pragma warning restore S1854 // Unused assignments should be removed

            try
            {
                results = await _cosmosDbRepository.RunStoredProcedureWithContinuationToken<TDocument>(storedProcedureName, storedProcedureParams);

                _logger.LogInformation(
                    GetPerformanceLogMessage(storedProcedureName, storedProcedureParams, performanceTimer, false));
            }
            catch (CosmosDbException cosmosDbException)
            {
                _logger.LogError(cosmosDbException, $"{nameof(CosmosDbServiceBase)}.{nameof(RunStoredProcedureAsync)} {storedProcedureName} failed.");

                if (cosmosDbException.StatusCode == HttpStatusCode.NotFound)
                {
                    await CreateStoredProcedure(storedProcedureName);
                    results = await _cosmosDbRepository.RunStoredProcedureWithContinuationToken<TDocument>(storedProcedureName, storedProcedureParams);

                    _logger.LogInformation(
                        GetPerformanceLogMessage(storedProcedureName, storedProcedureParams, performanceTimer, true));
                }
            }

            return results;

            string GetPerformanceLogMessage(
                string storedProcedureName,
                dynamic[] storedProcedureParams,
                Stopwatch performanceTimer,
                bool storedProcedureCreated)
            {
                performanceTimer.Stop();

                var logMessageBuilder = new StringBuilder();

                logMessageBuilder.AppendLine($"{nameof(CosmosDbServiceBase)}.{nameof(RunStoredProcedureAsync)}");
                logMessageBuilder.AppendLine($"Stored procedure name: {storedProcedureName}");
                logMessageBuilder.AppendLine($"Took: {performanceTimer.ElapsedMilliseconds:n0}ms");

                if (storedProcedureCreated)
                {
                    logMessageBuilder.AppendLine("The stored procedure was created prior to execution");
                }

                if (storedProcedureParams.Length > 0)
                {
                    logMessageBuilder.AppendLine("Params:");

                    foreach (var param in storedProcedureParams)
                    {
                        logMessageBuilder.AppendLine(param.ToString());
                    }
                }

                var logMessage = logMessageBuilder.ToString();
                return logMessage;
            }
        }

        /// <summary>Runs the query.</summary>
        /// <typeparam name="TValue">The type of the return value.</typeparam>
        /// <param name="queryName">Name of the query.</param>
        /// <param name="parameters">The query parameters.</param>
        /// <returns>The collection of results returned by the query.</returns>
        protected async Task<IEnumerable<TValue>> RunQueryAsync<TValue>(string queryName, params (string Name, object Value)[] parameters)
        {
            var performanceTimer = Stopwatch.StartNew();

#pragma warning disable S1854 // Unused assignments should be removed
            IEnumerable<TValue> result = default;
#pragma warning restore S1854 // Unused assignments should be removed

            try
            {
                var sqlQuery = GetSqlQuery(queryName);
                result = await _cosmosDbRepository.RunQuery<TValue>(sqlQuery, parameters);

                _logger.LogInformation(GetPerformanceLogMessage(queryName, performanceTimer));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"{nameof(CosmosDbServiceBase)}.{nameof(RunQueryAsync)} {queryName} query failed.");
            }

            return result;

            string GetPerformanceLogMessage(
                string queryName,
                Stopwatch performanceTimer)
            {
                performanceTimer.Stop();

                var logMessageBuilder = new StringBuilder();

                logMessageBuilder.AppendLine($"Call to CosmosDB {nameof(CosmosDbServiceBase)}.{nameof(RunQueryAsync)}");
                logMessageBuilder.AppendLine($"Query name: {queryName}");
                logMessageBuilder.AppendLine($"Took: {performanceTimer.ElapsedMilliseconds:n0}ms");

                var logMessage = logMessageBuilder.ToString();
                return logMessage;
            }
        }

        /// <summary>Ensures all stored procedures up to date asynchronous.</summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task EnsureAllStoredProceduresAreUpToDate()
        {
            var jsFileNameAndContents = _fileRepository.ReadMultipleFilesMatchingAPattern(".js");
            var failedOnSomeStoredProcedures = false;
            var failedStoredProcedures = new List<string>();

            foreach (var fileNameContentPair in jsFileNameAndContents)
            {
                var (storedProcedureName, requiredStoredProcedureBody) = (fileNameContentPair.Key, fileNameContentPair.Value);

                try
                {
                    var (isStoredProcedureUpToDate, storedProcedureExists) = await IsStoredProcedureUpToDate(
                        storedProcedureName,
                        requiredStoredProcedureBody);

                    if (!isStoredProcedureUpToDate)
                    {
                        if (storedProcedureExists)
                        {
                            await DeleteStoredProcedure(storedProcedureName);
                        }

                        await CreateStoredProcedure(storedProcedureName, requiredStoredProcedureBody);
                    }
                }
                catch (Exception)
                {
                    failedOnSomeStoredProcedures = true;
                    failedStoredProcedures.Add(storedProcedureName);
                }
            }

            if (failedOnSomeStoredProcedures)
            {
                throw new CosmosStoredProcedureManagementException($"Failed to ensure these stored procedures are up to date: {string.Join(", ", failedStoredProcedures)}");
            }
        }

        private string GetSqlQuery(string queryName)
        {
            var queryFileName = $"{queryName}.sql";

            try
            {
                return _fileRepository.ReadSingleFileContents(queryFileName);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Reading {queryFileName} failed.");

                throw;
            }
        }

        private async Task DeleteStoredProcedure(string storedProcedureName)
        {
            try
            {
                await _cosmosDbRepository.DeleteStoredProcedure(storedProcedureName);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Deleting {storedProcedureName} failed.");

                throw;
            }
        }

        private async Task CreateStoredProcedure(string storedProcedureName)
        {
            // Note: please ensure storedProcedureFileName is always storedProcedureName.js
            var storedProcedureFileName = $"{storedProcedureName}.js";
            string storedProcedureBody;

            try
            {
                storedProcedureBody = _fileRepository.ReadSingleFileContents(storedProcedureFileName);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Reading {storedProcedureFileName} failed.");

                throw;
            }

            if (storedProcedureBody != null)
            {
                await CreateStoredProcedure(storedProcedureName, storedProcedureBody);
            }
        }

        private async Task CreateStoredProcedure(string storedProcedureName, string storedProcedureBody)
        {
            try
            {
                await _cosmosDbRepository.CreateStoredProcedure(storedProcedureName, storedProcedureBody);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Creating {storedProcedureName} failed.");

                throw;
            }
        }

        private async Task<(bool isStoredProcedureUpToDate, bool storedProcedureExists)> IsStoredProcedureUpToDate(
            string storedProcedureName,
            string requiredStoredProcedureBody)
        {
            bool isStoredProcedureUpToDate = false, storedProcedureExists = false;
            try
            {
                var existingStoredProcedureBody = await _cosmosDbRepository.ReadStoredProcedure(storedProcedureName);
                isStoredProcedureUpToDate = requiredStoredProcedureBody == existingStoredProcedureBody;
                storedProcedureExists = true;
            }
            catch (CosmosDbException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    isStoredProcedureUpToDate = storedProcedureExists = false;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Reading {storedProcedureName} failed.");

                throw;
            }

            return (isStoredProcedureUpToDate, storedProcedureExists);
        }
    }
}