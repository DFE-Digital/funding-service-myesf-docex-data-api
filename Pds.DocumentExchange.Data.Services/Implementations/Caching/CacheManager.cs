using Pds.Core.Caching.Interfaces;
using Pds.Core.Utils.Helpers;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using System;

namespace Pds.DocumentExchange.Data.Services.Implementations.Caching
{
    /// <inheritdoc cref="ICacheManager"/>
    public class CacheManager : ICacheManager
    {
        private readonly ICacheService _cacheService;
        private readonly ICacheKeyBuilder _cacheKeyBuilder;
        private readonly ICacheOptionsProvider _cacheOptionsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheManager"/> class.
        /// </summary>
        /// <param name="cacheService">The cache service.</param>
        /// <param name="cacheKeyBuilder">The cache key builder.</param>
        /// <param name="cacheOptionsProvider">The cache options provider.</param>
        public CacheManager(
            ICacheService cacheService,
            ICacheKeyBuilder cacheKeyBuilder,
            ICacheOptionsProvider cacheOptionsProvider)
        {
            It.IsNull(cacheService)
               .AsGuard<ArgumentNullException>();

            It.IsNull(cacheKeyBuilder)
               .AsGuard<ArgumentNullException>();

            It.IsNull(cacheOptionsProvider)
               .AsGuard<ArgumentNullException>();

            _cacheService = cacheService;
            _cacheKeyBuilder = cacheKeyBuilder;
            _cacheOptionsProvider = cacheOptionsProvider;
        }

        /// <inheritdoc/>
        public ICacheService CacheService => _cacheService;

        /// <inheritdoc/>
        public ICacheKeyBuilder CacheKeyBuilder => _cacheKeyBuilder;

        /// <inheritdoc/>
        public ICacheOptionsProvider CacheOptionsProvider => _cacheOptionsProvider;
    }
}