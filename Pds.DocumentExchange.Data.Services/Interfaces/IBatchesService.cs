using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Interface providing methods to retrieve batches by a given organisation identifier.
    /// </summary>
    public interface IBatchesService
    {
        /// <summary>
        /// Gets a list of received files with Seen status for Agency users.
        /// </summary>
        /// <param name="allowedProducts">Allowed products collection.</param>
        /// <returns>A list of received files with Seen status for Agency users.</returns>
        Task<IEnumerable<DocumentWithViewedStatus>> GetReceivedFilesForAgencyWithSeenHistory(IEnumerable<Product> allowedProducts);

        /// <summary>
        /// Gets the documents to be deleted.
        /// </summary>
        /// <param name="direction">The exchange document direction.</param>
        /// <param name="ukprn">The UKPRN.</param>
        /// <param name="fileType">The file type.</param>
        /// <param name="year">The academic year.</param>
        /// <returns>A collection of BatchMetadata objects.</returns>
        Task<IEnumerable<BatchMetadata>> GetFileInfoForDelete(ExchangeDocumentDirection direction, int ukprn, string fileType, string year);

        /// <summary>
        /// Gets the received batches of a collection of organisations.
        /// </summary>
        /// <param name="organisationIdentifiers">The organisation identifiers collection.</param>
        /// <returns>A collection of BatchMetadata objects.</returns>
        Task<IEnumerable<BatchMetadata>> GetReceivedBatches(IEnumerable<OrganisationIdentifier> organisationIdentifiers);

        /// <summary>
        /// Gets the sent batches of a collection of organisations.
        /// </summary>
        /// <param name="organisationIdentifiers">The organisation identifiers collection.</param>
        /// <returns>A collection of BatchMetadata objects.</returns>
        Task<IEnumerable<BatchMetadata>> GetSentBatches(IEnumerable<OrganisationIdentifier> organisationIdentifiers);
    }
}