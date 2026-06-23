using Pds.Core.Common.Organisation.Models;

namespace Pds.DocumentExchange.Data.Services.DTOs.User
{
    /// <summary>
    /// Organisation info.
    /// </summary>
    public class OrganisationInfo
    {
        /// <summary>
        /// Gets or sets the organisation identifier.
        /// </summary>
        public OrganisationIdentifier OrganisationIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the organisation name.
        /// </summary>
        public string Name { get; set; }
    }
}