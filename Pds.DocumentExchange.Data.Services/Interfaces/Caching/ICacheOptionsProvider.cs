using Pds.Core.Caching.Models;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Caching
{
    /// <summary>
    /// Provider of caching options.
    /// </summary>
    public interface ICacheOptionsProvider
    {
        /// <summary>
        /// Gets the caching options for document lists.
        /// </summary>
        CacheOptions DocumentList { get; }

        /// <summary>
        /// Gets the caching options for document lists.
        /// </summary>
        CacheOptions DocumentValidation { get; }

        /// <summary>
        /// Gets the caching options for configuration objects.
        /// </summary>
        CacheOptions Configuration { get; }

        /// <summary>
        /// Gets the caching options for organisation types.
        /// </summary>
        CacheOptions OrganisationTypes { get; }

        /// <summary>
        /// Gets the caching options for all organisations.
        /// </summary>
        CacheOptions AllOrganisations { get; }
    }
}