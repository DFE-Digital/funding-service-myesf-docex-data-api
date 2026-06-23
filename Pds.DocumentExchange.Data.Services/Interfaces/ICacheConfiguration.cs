namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Cache configuration.
    /// </summary>
    public interface ICacheConfiguration
    {
        #region Configuration

        /// <summary>
        /// Gets a value indicating whether the configuration could be saved as null in the cache.
        /// </summary>
        bool ConfigurationCacheNullData { get; }

        /// <summary>
        /// Gets a value indicating whether the configuration should use the memory cache.
        /// </summary>
        bool ConfigurationUseMemoryCache { get; }

        /// <summary>
        /// Gets a value indicating whether the configuration should use the distributed cache.
        /// </summary>
        bool ConfigurationUseDistributedCache { get; }

        /// <summary>
        /// Gets the memory cache configuration time in seconds.
        /// </summary>
        int ConfigurationMemoryCacheTimeSeconds { get; }

        /// <summary>
        /// Gets the distributed cache configuration time in minutes.
        /// </summary>
        int ConfigurationDistributedCacheTimeMinutes { get; }

        /// <summary>
        /// Gets a value indicating whether the configuration data should be compressed.
        /// </summary>
        bool ConfigurationUseCompression { get; }

        #endregion


        #region Document list

        /// <summary>
        /// Gets a value indicating whether a document list could be saved as null in the cache.
        /// </summary>
        bool DocumentListCacheNullData { get; }

        /// <summary>
        /// Gets a value indicating whether the document list should use the memory cache.
        /// </summary>
        bool DocumentListUseMemoryCache { get; }

        /// <summary>
        /// Gets a value indicating whether the document list should use the distributed cache.
        /// </summary>
        bool DocumentListUseDistributedCache { get; }

        /// <summary>
        /// Gets the document validation cache time in minutes.
        /// </summary>
        int DocumentListCacheTimeMinutes { get; }

        /// <summary>
        /// Gets a value indicating whether the document list data should be compressed.
        /// </summary>
        bool DocumentListUseCompression { get; }

        #endregion


        #region Document validation

        /// <summary>
        /// Gets a value indicating whether a document validation could be saved as null in the cache.
        /// </summary>
        bool DocumentValidationCacheNullData { get; }

        /// <summary>
        /// Gets a value indicating whether the document validation should use the memory cache.
        /// </summary>
        bool DocumentValidationUseMemoryCache { get; }

        /// <summary>
        /// Gets a value indicating whether the document validation should use the distributed cache.
        /// </summary>
        bool DocumentValidationUseDistributedCache { get; }

        /// <summary>
        /// Gets the configuration cache time in minutes.
        /// </summary>
        int DocumentValidationCacheTimeMinutes { get; }

        /// <summary>
        /// Gets a value indicating whether the document validation data should be compressed.
        /// </summary>
        bool DocumentValidationUseCompression { get; }

        #endregion


        #region Organisation types

        /// <summary>
        /// Gets a value indicating whether an organisation type could be saved as null in the cache.
        /// </summary>
        bool OrganisationTypesCacheNullData { get; }

        /// <summary>
        /// Gets a value indicating whether the organisation types should use the memory cache.
        /// </summary>
        bool OrganisationTypesUseMemoryCache { get; }

        /// <summary>
        /// Gets a value indicating whether the organisation types should use the distributed cache.
        /// </summary>
        bool OrganisationTypesUseDistributedCache { get; }

        /// <summary>
        /// Gets the organisation types cache time in days.
        /// </summary>
        int OrganisationTypesCacheTimeDays { get; }

        /// <summary>
        /// Gets a value indicating whether the organisation types data should be compressed.
        /// </summary>
        bool OrganisationTypesUseCompression { get; }

        #endregion


        #region All Organisations

        /// <summary>
        /// Gets a value indicating whether an organisations could be saved as null in the cache.
        /// </summary>
        bool AllOrganisationsCacheNullData { get; }

        /// <summary>
        /// Gets a value indicating whether the organisations should use the memory cache.
        /// </summary>
        bool AllOrganisationsUseMemoryCache { get; }

        /// <summary>
        /// Gets a value indicating whether the organisations should use the distributed cache.
        /// </summary>
        bool AllOrganisationsUseDistributedCache { get; }

        /// <summary>
        /// Gets the organisation memory cache time in minutes.
        /// </summary>
        int AllOrganisationsMemoryCacheTimeMinutes { get; }

        /// <summary>
        /// Gets the organisation distributed cache time in minutes.
        /// </summary>
        int AllOrganisationsDistributedCacheTimeMinutes { get; }

        /// <summary>
        /// Gets a value indicating whether the organisation data should be compressed.
        /// </summary>
        bool AllOrganisationUseCompression { get; }

        #endregion
    }
}