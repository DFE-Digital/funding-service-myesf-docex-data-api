using Pds.Core.Common.Organisation.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.FDS
{
    /// <summary>
    /// Interface providing methods to call fds api service.
    /// </summary>
    public interface IOrganisationService
    {
        /// <summary>
        /// Gets the organisation with the given identifier.
        /// </summary>
        /// <param name="ukprn">The ukprn to lookup.</param>
        /// <returns>The organisation with the given identifier.</returns>
        Task<Organisation> GetOrganisation(string ukprn);

        /// <summary>
        /// Gets the organisation info with the given name.
        /// </summary>
        /// <param name="name">The name to lookup.</param>
        /// <param name="maxResults">The max results.</param>
        /// <returns>The organisation info.</returns>
        Task<IEnumerable<Organisation>> GetOrganisationByName(string name, int maxResults);

        /// <summary>
        /// Get all the organisation info.
        /// </summary>
        /// <returns>List of organisations.</returns>
        Task<IDictionary<string, Organisation>> GetAllOrganisations();
    }
}