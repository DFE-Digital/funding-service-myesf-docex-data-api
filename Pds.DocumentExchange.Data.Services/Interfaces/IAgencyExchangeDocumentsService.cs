using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Interface providing a method to get the agency teams' exchange documents.
    /// </summary>
    public interface IAgencyExchangeDocumentsService
    {
        /// <summary>
        /// Gets a list of received files with Seen status for Agency users.
        /// </summary>
        /// <param name="teams">The agency teams.</param>
        /// <returns>A list of received files with Seen status for Agency users.</returns>
        Task<IEnumerable<DocumentWithViewedStatus>> GetReceivedFilesForAgencyWithSeenHistory(IEnumerable<string> teams);

        /// <summary>
        /// Gets the agency files delete information.
        /// </summary>
        /// <param name="direction">The documents direction.</param>
        /// <param name="ukprn">The UKPRN.</param>
        /// <param name="fileType">The file type.</param>
        /// <param name="year">The year.</param>
        /// <returns>A collection with the delete files information.</returns>
        Task<ListResult<ExchangeDocument>> GetDeleteFiles(ExchangeDocumentDirection direction, int ukprn, string fileType, string year);

        /// <summary>
        /// Gets the exchange documents of the given agency teams.
        /// </summary>
        /// <param name="teams">The agency teams.</param>
        /// <param name="direction">The documents direction.</param>
        /// <returns>A collection of ExchangeDocument objects.</returns>
        Task<IEnumerable<ExchangeDocument>> GetExchangeDocuments(IEnumerable<string> teams, ExchangeDocumentDirection direction);
    }
}