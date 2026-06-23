using Pds.Core.Caching.Models;
using Pds.Core.Utils.Helpers;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using System;

namespace Pds.DocumentExchange.Data.Services.Implementations.Caching
{
    /// <inheritdoc cref="ICacheOptionsProvider"/>
    public class CacheOptionsProvider : ICacheOptionsProvider
    {
        private readonly ICacheConfiguration _cacheConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheOptionsProvider"/> class.
        /// </summary>
        /// <param name="cacheConfiguration">The cache configuration.</param>
        public CacheOptionsProvider(ICacheConfiguration cacheConfiguration)
        {
            It.IsNull(cacheConfiguration)
                .AsGuard<ArgumentNullException>();

            _cacheConfiguration = cacheConfiguration;
        }

        /// <inheritdoc/>
        public CacheOptions DocumentList => new CacheOptions
        {
            CacheNullData = _cacheConfiguration.DocumentListCacheNullData,
            UseMemoryCache = _cacheConfiguration.DocumentListUseMemoryCache,
            MemoryCacheTTL = TimeSpan.FromMinutes(_cacheConfiguration.DocumentListCacheTimeMinutes),
            UseDistributedCache = _cacheConfiguration.DocumentListUseDistributedCache,
            DistributedCacheTTL = TimeSpan.FromMinutes(_cacheConfiguration.DocumentListCacheTimeMinutes),
            UseCompression = _cacheConfiguration.DocumentListUseCompression
        };

        /// <inheritdoc/>
        public CacheOptions DocumentValidation => new CacheOptions
        {
            CacheNullData = _cacheConfiguration.DocumentValidationCacheNullData,
            UseMemoryCache = _cacheConfiguration.DocumentValidationUseMemoryCache,
            MemoryCacheTTL = TimeSpan.FromMinutes(_cacheConfiguration.DocumentValidationCacheTimeMinutes),
            UseDistributedCache = _cacheConfiguration.DocumentValidationUseDistributedCache,
            DistributedCacheTTL = TimeSpan.FromMinutes(_cacheConfiguration.DocumentValidationCacheTimeMinutes),
            UseCompression = _cacheConfiguration.DocumentValidationUseCompression
        };

        /// <inheritdoc/>
        public CacheOptions Configuration => new CacheOptions
        {
            CacheNullData = _cacheConfiguration.ConfigurationCacheNullData,
            UseMemoryCache = _cacheConfiguration.ConfigurationUseMemoryCache,
            MemoryCacheTTL = TimeSpan.FromSeconds(_cacheConfiguration.ConfigurationMemoryCacheTimeSeconds),
            UseDistributedCache = _cacheConfiguration.ConfigurationUseDistributedCache,
            DistributedCacheTTL = TimeSpan.FromMinutes(_cacheConfiguration.ConfigurationDistributedCacheTimeMinutes),
            UseCompression = _cacheConfiguration.ConfigurationUseCompression
        };

        /// <inheritdoc/>
        public CacheOptions OrganisationTypes => new CacheOptions
        {
            CacheNullData = _cacheConfiguration.OrganisationTypesCacheNullData,
            UseMemoryCache = _cacheConfiguration.OrganisationTypesUseMemoryCache,
            MemoryCacheTTL = TimeSpan.FromDays(_cacheConfiguration.OrganisationTypesCacheTimeDays),
            UseDistributedCache = _cacheConfiguration.OrganisationTypesUseDistributedCache,
            DistributedCacheTTL = TimeSpan.FromDays(_cacheConfiguration.OrganisationTypesCacheTimeDays),
            UseCompression = _cacheConfiguration.OrganisationTypesUseCompression
        };

        /// <inheritdoc/>
        public CacheOptions AllOrganisations => new CacheOptions
        {
            CacheNullData = _cacheConfiguration.AllOrganisationsCacheNullData,
            UseMemoryCache = _cacheConfiguration.AllOrganisationsUseMemoryCache,
            MemoryCacheTTL = TimeSpan.FromMinutes(_cacheConfiguration.AllOrganisationsMemoryCacheTimeMinutes),
            UseDistributedCache = _cacheConfiguration.AllOrganisationsUseDistributedCache,
            DistributedCacheTTL = TimeSpan.FromMinutes(_cacheConfiguration.AllOrganisationsDistributedCacheTimeMinutes),
            UseCompression = _cacheConfiguration.AllOrganisationUseCompression
        };
    }
}