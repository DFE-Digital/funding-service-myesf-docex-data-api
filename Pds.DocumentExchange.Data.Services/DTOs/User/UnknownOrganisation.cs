using Pds.Core.Common.Organisation.Models;
using Pds.Services.Common.Helpers;

namespace Pds.DocumentExchange.Data.Services.DTOs.User
{
    /// <summary>
    /// Represents an unknown organisation.
    /// </summary>
    public class UnknownOrganisation : Organisation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownOrganisation"/> class.
        /// </summary>
        /// <param name="organisationIdentifier">The unknown organisation identifier.</param>
        public UnknownOrganisation(OrganisationIdentifier organisationIdentifier)
        {
            Name = "Unknown organisation";

            OrganisationType = "Unknown organisation type";
            OrganisationTypeDisplay = new UnknownOrganisationTypeDisplay();

            OrganisationSubType = "Unknown organisation subtype";
            OrganisationSubTypeDisplay = new UnknownOrganisationTypeDisplay();

            Identifiers = new[] { organisationIdentifier };
            ChildOrganisations = Collection.EmptyAndReadOnly<Organisation>();
        }
    }
}