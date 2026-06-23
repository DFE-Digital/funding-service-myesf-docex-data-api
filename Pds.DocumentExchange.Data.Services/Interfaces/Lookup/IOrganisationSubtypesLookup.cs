using Pds.Core.Common.Organisation.Models;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Lookup
{
    /// <summary>
    /// The organisation subtypes lookup interface.
    /// </summary>
    public interface IOrganisationSubtypesLookup : ILookup<DisplayValues, string>
    {
        /// <summary>
        /// Includes the organisation type and subtype displays from the specified organisation.
        /// </summary>
        /// <param name="organisation">The organisation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task LoadOrganisationDisplays(Organisation organisation);

        /// <summary>
        /// Gets the organisation type display by the specified subtype identifier.
        /// </summary>
        /// <param name="organisationSubtypeIdentifier">The organisation subtype identifier.</param>
        /// <returns>The organisation type identifier.</returns>
        Task<DisplayValues> GetOrganisationTypeDisplay(string organisationSubtypeIdentifier);
    }
}