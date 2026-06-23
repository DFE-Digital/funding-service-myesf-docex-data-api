using Pds.DocumentExchange.Data.Services.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Provides methods to select and filter documents.
    /// </summary>
    public interface IExchangeDocumentSelector
    {
        /// <summary>
        /// Gets the agency's teams documents based on the specified options.
        /// </summary>
        /// <param name="teams">The agency teams.</param>
        /// <param name="options">The exchange list document options.</param>
        /// <returns>A list result containing the filtered and paged exchanged documents
        /// along with the updated filters.</returns>
        Task<ListResult<ExchangeDocument>> GetAgencyTeamFiles(
            IEnumerable<string> teams,
            ExchangeListDocumentOptions options);

        /// <summary>
        /// Gets an organisation's documents based on the specified options.
        /// </summary>
        /// <param name="options">The exchange list organisation document options.</param>
        /// <returns>A list result containing the filtered and paged exchanged documents
        /// along with the updated filters.</returns>
        Task<ListResult<ExchangeDocument>> GetOrganisationFiles(ExchangeListOrganisationDocumentOptions options);

        /// <summary>
        /// Gets the agency team exchange documents.
        /// </summary>
        /// <param name="teams">The teams.</param>
        /// <param name="documentReferences">The document references.</param>
        /// <returns>List of exchange documents.</returns>
        Task<IEnumerable<ExchangeDocument>> GetAgencyTeamExchangeDocuments(IEnumerable<string> teams, IEnumerable<DocumentReference> documentReferences);
    }
}