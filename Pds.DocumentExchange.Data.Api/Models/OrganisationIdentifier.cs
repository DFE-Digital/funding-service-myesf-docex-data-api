using Pds.DocumentExchange.Data.Api.Enums;

namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>The organisation identifier.</summary>
    public class OrganisationIdentifier
    {
        /// <summary>
        /// Gets or sets the identifier type.
        /// </summary>
        public OrganisationIdentifierType Type { get; set; }

        /// <summary>
        /// Gets or sets the identifier value.
        /// </summary>
        public string Value { get; set; }
    }
}