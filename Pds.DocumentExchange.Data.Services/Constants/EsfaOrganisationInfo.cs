using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;

namespace Pds.DocumentExchange.Data.Services.Constants
{
    /// <summary>
    /// Class containing the ESFA organisation info.
    /// </summary>
    public static class EsfaOrganisationInfo
    {
        /// <summary>
        /// Gets the ESFA organisation identifier.
        /// </summary>
        public static OrganisationIdentifier Identifier { get; } = new OrganisationIdentifier
        {
            Type = OrganisationIdentifierType.Ukprn,
            Value = "-999"
        };
    }
}