using Pds.Core.Common.Organisation.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Lookup
{
    /// <summary>
    /// The organisations lookup interface.
    /// </summary>
    public interface IOrganisationsLookup : ILookup<Organisation, OrganisationIdentifier>
    {
        /// <summary>
        /// Gets all organisation identifiers which are related to the specified identifier parameter:
        /// 1. The identifier itself.
        /// 2. Any other identifiers the organisation could have.
        /// 3. The child organisations identifiers.
        /// </summary>
        /// <param name="identifier">The organisation identifier.</param>
        /// <returns>A collection containing all the related organisation identifiers.</returns>
        Task<IEnumerable<OrganisationIdentifier>> GetSelfAndChildUkprns(OrganisationIdentifier identifier);

        /// <summary>
        /// Get all the organisation info.
        /// </summary>
        /// <returns>List of organisations.</returns>
        Task<IDictionary<string, Organisation>> GetAllOrganisations();
    }
}