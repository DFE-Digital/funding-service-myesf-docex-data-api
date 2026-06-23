using Pds.Core.Caching.Interfaces;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Caching
{
    /// <summary>
    /// Cache manager.
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// Gets the cache service.
        /// </summary>
        ICacheService CacheService { get; }

        /// <summary>
        /// Gets the cache key builder.
        /// </summary>
        ICacheKeyBuilder CacheKeyBuilder { get; }

        /// <summary>
        /// Gets the cache options provider.
        /// </summary>
        ICacheOptionsProvider CacheOptionsProvider { get; }
    }
}