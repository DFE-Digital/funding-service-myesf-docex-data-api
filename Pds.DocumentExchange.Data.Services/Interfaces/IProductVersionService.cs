using Pds.Core.Common.Organisation.Models;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Interface providing methods to get product versions.
    /// </summary>
    public interface IProductVersionService
    {
        /// <summary>
        /// Gets the product version for a specific organisation.
        /// </summary>
        /// <param name="organisationIdentifier">The organisation identifier.</param>
        /// <param name="productIdentifier">The product identifier.</param>
        /// <returns>The product version.</returns>
        Task<int> GetCurrentProductVersion(OrganisationIdentifier organisationIdentifier, int productIdentifier);
    }
}