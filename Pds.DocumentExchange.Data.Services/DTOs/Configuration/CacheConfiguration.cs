using Pds.DocumentExchange.Data.Services.Interfaces;

namespace Pds.DocumentExchange.Data.Services.DTOs.Configuration
{
    /// <inheritdoc cref="ICacheConfiguration"/>
    public class CacheConfiguration : ICacheConfiguration
    {
        #region Document list

        /// <inheritdoc/>
        public bool DocumentListCacheNullData { get; set; } = true;

        /// <inheritdoc/>
        public bool DocumentListUseMemoryCache { get; set; } = true;

        /// <inheritdoc/>
        public bool DocumentListUseDistributedCache { get; set; } = true;

        /// <inheritdoc/>
        public int DocumentListCacheTimeMinutes { get; set; } = 5;

        /// <inheritdoc/>
        public bool DocumentListUseCompression { get; set; } = true;

        #endregion


        #region Document validation

        /// <inheritdoc/>
        public bool DocumentValidationCacheNullData { get; set; } = false;

        /// <inheritdoc/>
        public bool DocumentValidationUseMemoryCache { get; set; } = true;

        /// <inheritdoc/>
        public bool DocumentValidationUseDistributedCache { get; set; } = true;

        /// <inheritdoc/>
        public int DocumentValidationCacheTimeMinutes { get; set; } = 60 * 24;

        /// <inheritdoc/>
        public bool DocumentValidationUseCompression { get; set; } = false;

        #endregion


        #region Configuration

        /// <inheritdoc/>
        public bool ConfigurationCacheNullData { get; set; } = false;

        /// <inheritdoc/>
        public bool ConfigurationUseMemoryCache { get; set; } = true;

        /// <inheritdoc/>
        public bool ConfigurationUseDistributedCache { get; set; } = true;

        /// <inheritdoc/>
        public int ConfigurationMemoryCacheTimeSeconds { get; set; } = 10;

        /// <inheritdoc/>
        public int ConfigurationDistributedCacheTimeMinutes { get; set; } = 60;

        /// <inheritdoc/>
        public bool ConfigurationUseCompression { get; set; } = false;

        #endregion


        #region Organisation types

        /// <inheritdoc/>
        public bool OrganisationTypesCacheNullData { get; set; } = true;

        /// <inheritdoc/>
        public bool OrganisationTypesUseMemoryCache { get; set; } = true;

        /// <inheritdoc/>
        public bool OrganisationTypesUseDistributedCache { get; set; } = true;

        /// <inheritdoc/>
        public int OrganisationTypesCacheTimeDays { get; set; } = 30;

        /// <inheritdoc/>
        public bool OrganisationTypesUseCompression { get; set; } = false;

        #endregion


        #region All Organisations

        /// <inheritdoc/>
        public bool AllOrganisationsCacheNullData { get; set; } = true;

        /// <inheritdoc/>
        public bool AllOrganisationsUseMemoryCache { get; set; } = true;

        /// <inheritdoc/>
        public bool AllOrganisationsUseDistributedCache { get; set; } = false;

        /// <inheritdoc/>
        public int AllOrganisationsMemoryCacheTimeMinutes { get; set; } = 60;

        /// <inheritdoc/>
        public int AllOrganisationsDistributedCacheTimeMinutes { get; set; } = 60;

        /// <inheritdoc/>
        public bool AllOrganisationUseCompression { get; set; } = false;

        #endregion
    }
}