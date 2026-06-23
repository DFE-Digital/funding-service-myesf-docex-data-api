using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Interface exposing methods to convert from batch metadata records to exchange documents.
    /// </summary>
    public interface IBatchToExchangeDocumentConverter
    {
        /// <summary>
        /// Coverts a single file metadata record to an exchange document for the agency.
        /// </summary>
        /// <param name="parentBatchIdentifier">The parent batch identifier.</param>
        /// <param name="batchIdentifier">The batch identifier.</param>
        /// <param name="fileMetadata">The file metadata object.</param>
        /// <param name="direction">The exchange document direction.</param>
        /// <returns>The exchange document.</returns>
        public Task<ExchangeDocument> ConvertToAgencyExchangeDocument(
           string parentBatchIdentifier,
           string batchIdentifier,
           FileMetadata fileMetadata,
           ExchangeDocumentDirection direction);

        /// <summary>
        /// Converts a collection of batch metadata objects to exchange documents for the agency.
        /// </summary>
        /// <param name="batches">The batches collection.</param>
        /// <param name="direction">The exchange document direction.</param>
        /// <returns>A collection of exchange documents.</returns>
        Task<IEnumerable<ExchangeDocument>> ConvertToAgencyExchangeDocuments(
            IEnumerable<BatchMetadata> batches,
            ExchangeDocumentDirection direction);

        /// <summary>
        /// Converts a collection of batch metadata objects to exchange documents for an organisation.
        /// </summary>
        /// <param name="batches">The batches collection.</param>
        /// <param name="direction">The exchange document direction.</param>
        /// <returns>A collection of exchange documents.</returns>
        Task<IEnumerable<ExchangeDocument>> ConvertToOrganisationExchangeDocuments(
            IEnumerable<BatchMetadata> batches,
            ExchangeDocumentDirection direction);
    }
}