using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Interface providing a method to get an organisation's exchange documents.
    /// </summary>
    public interface IOrganisationExchangeDocumentsService
    {
        /// <summary>
        /// Gets the exchange documents of an organisation.
        /// </summary>
        /// <param name="organisationIdentifier">The organisation identifier.</param>
        /// <param name="direction">The documents direction.</param>
        /// <returns>A collection of ExchangeDocument objects.</returns>
        Task<IEnumerable<ExchangeDocument>> GetExchangeDocuments(OrganisationIdentifier organisationIdentifier, ExchangeDocumentDirection direction);
    }
}