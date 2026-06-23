using Pds.DocumentExchange.Data.Services.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Interface providing a method to delete a collection of documents.
    /// </summary>
    public interface IDocumentDeletion
    {
        /// <summary>
        /// Deletes a collection of documents.
        /// </summary>
        /// <param name="exchangeDocumentDeleteRequest">The exchange document delete request.</param>
        /// <returns>A collection of exchange document delete results.</returns>
        Task<IEnumerable<ExchangeDocument>> DeleteDocuments(ExchangeDocumentDeleteRequest exchangeDocumentDeleteRequest);

        /// <summary>
        /// Deletes a single document.
        /// </summary>
        /// <param name="exchangeDocumentDeleteRequest">The exchange document delete request.</param>
        /// <returns>A collection of exchange document delete results.</returns>
        Task<ExchangeDocument> DeleteDocument(ExchangeDocumentDeleteRequest exchangeDocumentDeleteRequest);
    }
}