using Pds.DocumentExchange.Data.Repository.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Repository.Interfaces
{
    /// <summary>The IAzureCosmosDbRepository interface, exposes the required methods out of Azure Cosmos Db.</summary>
    public interface IAzureCosmosDbRepository
    {
        /// <summary>Runs the stored procedure.</summary>
        /// <typeparam name="TValue">The type of the return value.</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="storedProcedureParams">The stored procedure parameters.</param>
        /// <returns>The result of running the stored procedure.</returns>
        Task<TValue> RunStoredProcedure<TValue>(string storedProcedureName, params dynamic[] storedProcedureParams);

        /// <summary>Runs the stored procedure with continuation token.</summary>
        /// <typeparam name="TValue">The type of the return value.</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="storedProcedureParams">The stored procedure parameters.</param>
        /// <returns>The result of running the stored procedure.</returns>
        Task<IEnumerable<TValue>> RunStoredProcedureWithContinuationToken<TValue>(string storedProcedureName, dynamic[] storedProcedureParams);

        /// <summary>Runs the query.</summary>
        /// <typeparam name="TDocument">The type of the return values.</typeparam>
        /// <param name="sqlQuery">The SQL query to run.</param>
        /// <param name="parameters">The query parameters.</param>
        /// <returns>The collection of documents returned by the query.</returns>
        Task<IEnumerable<TDocument>> RunQuery<TDocument>(string sqlQuery, params (string Name, object Value)[] parameters);

        /// <summary>Creates the stored procedure.</summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="storedProcedureBody">The stored procedure body.</param>
        /// <returns>The result of running the stored procedure.</returns>
        Task CreateStoredProcedure(string storedProcedureName, string storedProcedureBody);

        /// <summary>
        /// Creates a new document.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="document">The document.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateDocument<TDocument>(TDocument document)
             where TDocument : CosmosDbDocument;

        /// <summary>
        /// Upserts a document to the database.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="document">The document.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpsertDocument<TDocument>(TDocument document)
            where TDocument : CosmosDbDocument;

        /// <summary>
        /// Gets a document by ID.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="id">The document ID.</param>
        /// <returns>An object of type TValue.</returns>
        Task<TDocument> GetDocumentById<TDocument>(string id)
            where TDocument : class;

        /// <summary>Reads the stored procedure.</summary>
        /// <param name="storedProcedureName">Name of the file.</param>
        /// <returns>A string containing the stored procedure body from azure cosmos db.</returns>
        Task<string> ReadStoredProcedure(string storedProcedureName);

        /// <summary>Deletes the stored procedure.</summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <returns>An asynchronously awaitable task.</returns>
        Task DeleteStoredProcedure(string storedProcedureName);

        /// <summary>
        /// Performs a bulk insert of the collection of documents.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents collection.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task BulkImport<TDocument>(IEnumerable<TDocument> documents)
            where TDocument : CosmosDbDocument;
    }
}