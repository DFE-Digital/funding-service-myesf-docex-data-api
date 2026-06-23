using Pds.Core.Common.Organisation.Models;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// Contains options for selecting organisation documents.
    /// </summary>
    public class ExchangeListOrganisationDocumentOptions : ExchangeListDocumentOptions
    {
        /// <summary>
        /// Gets or sets the organisation identifier.
        /// </summary>
        public OrganisationIdentifier OrganisationIdentifier { get; set; }
    }
}