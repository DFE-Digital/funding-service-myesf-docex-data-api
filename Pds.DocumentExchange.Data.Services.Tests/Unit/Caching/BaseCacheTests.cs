using Moq;
using Pds.Core.Caching.Interfaces;
using Pds.Core.Caching.Models;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Caching
{
    public abstract class BaseCacheTests
    {
        protected ICacheManager CacheManager { get; }

        private readonly ICacheKeyBuilder _cacheKeyBuilder = Mock.Of<ICacheKeyBuilder>(MockBehavior.Strict);

        private readonly ICacheOptionsProvider _cacheOptionsProvider =
            Mock.Of<ICacheOptionsProvider>(MockBehavior.Strict);

        private readonly ICacheService _cacheService = Mock.Of<ICacheService>(MockBehavior.Strict);

        private Mock<ICacheKeyBuilder> MockCacheKeyBuilder => Mock.Get(_cacheKeyBuilder);

        private Mock<ICacheOptionsProvider> MockCacheOptionsProvider => Mock.Get(_cacheOptionsProvider);

        private Mock<ICacheService> MockCacheService => Mock.Get(_cacheService);

        protected BaseCacheTests()
        {
            CacheManager = Mock.Of<ICacheManager>();

            var mockCacheManager = Mock.Get(CacheManager);

            mockCacheManager
                .SetupGet(c => c.CacheKeyBuilder)
                .Returns(_cacheKeyBuilder);

            mockCacheManager
                .SetupGet(c => c.CacheOptionsProvider)
                .Returns(_cacheOptionsProvider);

            mockCacheManager
                .SetupGet(c => c.CacheService)
                .Returns(_cacheService);
        }

        protected IEnumerable<string> SetupBuildCacheKeys(
            params Expression<Func<ICacheKeyBuilder, string>>[] buildCacheKeys)
        {
            foreach (var buildCacheKey in buildCacheKeys)
            {
                var fakeCacheKey = $"fake_key_{Guid.NewGuid()}";

                MockCacheKeyBuilder
                    .Setup(buildCacheKey)
                    .Returns(fakeCacheKey)
                    .Verifiable();

                yield return fakeCacheKey;
            }
        }

        protected CacheOptions SetupGetCacheOptions(
            Expression<Func<ICacheOptionsProvider, CacheOptions>> getCacheOptions)
        {
            var fakeCachingOptions
                = new CacheOptions
                {
                    CacheNullData = false,
                    MemoryCacheTTL = TimeSpan.FromSeconds(new Random().Next(1, 100))
                };

            MockCacheOptionsProvider
                .SetupGet(getCacheOptions)
                .Returns(fakeCachingOptions)
                .Verifiable();

            return fakeCachingOptions;
        }

        protected void SetupCacheGetMock<TCached>(
            Expression<Func<ICacheOptionsProvider, CacheOptions>> getCacheOptions,
            Expression<Func<ICacheKeyBuilder, string>> buildCacheKey,
            TCached result)
        {
            SetupCacheGetMocks(getCacheOptions, new[] { buildCacheKey }, new[] { result });
        }

        protected void SetupCacheGetMocks<TCached>(
            Expression<Func<ICacheOptionsProvider, CacheOptions>> getCacheOptions,
            params Expression<Func<ICacheKeyBuilder, string>>[] buildCacheKeys)
        {
            SetupCacheGetMocks<TCached>(getCacheOptions, buildCacheKeys, null);
        }

        protected void SetupCacheGetMocks<TCached>(
            Expression<Func<ICacheOptionsProvider, CacheOptions>> getCacheOptions,
            Expression<Func<ICacheKeyBuilder, string>>[] buildCacheKeys,
            IEnumerable<TCached> results)
        {
            var cacheOptions = SetupGetCacheOptions(getCacheOptions);
            var cacheKeys = SetupBuildCacheKeys(buildCacheKeys);

            SetupCacheGetMocks(cacheOptions, cacheKeys, results);
        }

        protected void SetupCacheGetMocks<TCached>(
            CacheOptions cacheOptions,
            IEnumerable<string> cacheKeys,
            IEnumerable<TCached> results)
        {
            void SetupCacheGetInvokingCapturedFunction(string cacheKey, CacheOptions options)
            {
                MockCacheService.Setup(s => s.Get(cacheKey, It.IsAny<Func<Task<TCached>>>(), options))
                    .Returns((string key, Func<Task<TCached>> captured, CacheOptions _) => captured())
                    .Verifiable();
            }

            void SetupCacheGetReturningResult(string cacheKey, CacheOptions options, TCached result)
            {
                MockCacheService.Setup(s => s.Get(cacheKey, It.IsAny<Func<Task<TCached>>>(), options))
                    .ReturnsAsync(result)
                    .Verifiable();
            }

            if (results == null)
            {
                SetupCacheActionForAllKeys(SetupCacheGetInvokingCapturedFunction, cacheOptions, cacheKeys);
            }
            else
            {
                SetupCacheActionForAllKeys(SetupCacheGetReturningResult, cacheOptions, cacheKeys, results);
            }
        }

        protected void SetupCacheTryGetMocks<TCached>(
            Expression<Func<ICacheOptionsProvider, CacheOptions>> getCacheOptions,
            Expression<Func<ICacheKeyBuilder, string>>[] buildCacheKeys,
            IEnumerable<(bool exists, TCached result)> results)
        {
            var cacheOptions = SetupGetCacheOptions(getCacheOptions);
            var cacheKeys = SetupBuildCacheKeys(buildCacheKeys);

            SetupCacheTryGetMocks(cacheOptions, cacheKeys, results);
        }

        protected void SetupCacheTryGetMocks<TCached>(
            CacheOptions cacheOptions,
            IEnumerable<string> cacheKeys,
            IEnumerable<(bool exists, TCached result)> results)
        {
            void SetupCacheTryGet(string cacheKey, CacheOptions options, (bool exists, TCached result) result)
            {
                MockCacheService
                    .Setup(s => s.TryGet(cacheKey, It.IsAny<Func<Task<TCached>>>(), cacheOptions))
                    .ReturnsAsync(
                        (string key, Func<Task<TCached>> captured, CacheOptions _) =>
                        {
                            captured?.Invoke().GetAwaiter().GetResult();

                            return result;
                        })
                    .Verifiable();
            }

            SetupCacheActionForAllKeys(SetupCacheTryGet, cacheOptions, cacheKeys, results);
        }

        protected void SetupCacheSetMocks<TCached>(
            Expression<Func<ICacheOptionsProvider, CacheOptions>> getCacheOptions,
            params Expression<Func<ICacheKeyBuilder, string>>[] buildCacheKeys)
        {
            var cacheOptions = SetupGetCacheOptions(getCacheOptions);
            var cacheKeys = SetupBuildCacheKeys(buildCacheKeys).ToArray();

            SetupCacheSetMocks<TCached>(cacheOptions, cacheKeys);
        }

        protected void SetupCacheSetMocks<TCached>(
            CacheOptions cacheOptions,
            params string[] cacheKeys)
        {
            void SetupCacheSet(string cacheKey, CacheOptions options)
            {
                MockCacheService.Setup(s => s.Set(cacheKey, It.IsAny<Func<Task<TCached>>>(), options))
                    .Returns(
                        (string key, Func<Task<TCached>> captured, CacheOptions _) =>
                        {
                            captured?.Invoke().GetAwaiter().GetResult();

                            return Task.CompletedTask;
                        })
                    .Verifiable();
            }

            SetupCacheActionForAllKeys(SetupCacheSet, cacheOptions, cacheKeys);
        }

        protected void SetupCacheRemoveMocks(
            Expression<Func<ICacheOptionsProvider, CacheOptions>> getCacheOptions,
            params Expression<Func<ICacheKeyBuilder, string>>[] buildCacheKeys)
        {
            var cacheOptions = SetupGetCacheOptions(getCacheOptions);
            var cacheKeys = SetupBuildCacheKeys(buildCacheKeys).ToArray();

            SetupCacheRemoveMocks(cacheOptions, cacheKeys);
        }

        protected void SetupCacheRemoveMocks(
            CacheOptions cacheOptions,
            params string[] cacheKeys)
        {
            void SetupCacheRemove(string cacheKey, CacheOptions options)
            {
                MockCacheService.Setup(s => s.Remove(cacheKey, cacheOptions))
                    .Returns(Task.CompletedTask)
                    .Verifiable();
            }

            SetupCacheActionForAllKeys(SetupCacheRemove, cacheOptions, cacheKeys);
        }

        protected void VerifyCacheMocks()
        {
            Mock.Verify(MockCacheKeyBuilder, MockCacheOptionsProvider, MockCacheService);
        }

        private void SetupCacheActionForAllKeys<TResult>(
            Action<string, CacheOptions, TResult> setupCacheAction,
            CacheOptions cacheOptions,
            IEnumerable<string> cacheKeys,
            IEnumerable<TResult> results)
        {
            var entries = cacheKeys.Zip(results);

            foreach (var (cacheKey, result) in entries)
            {
                setupCacheAction(cacheKey, cacheOptions, result);
            }
        }

        private void SetupCacheActionForAllKeys(
            Action<string, CacheOptions> setupCacheAction,
            CacheOptions cacheOptions,
            IEnumerable<string> cacheKeys)
        {
            foreach (var cacheKey in cacheKeys)
            {
                setupCacheAction(cacheKey, cacheOptions);
            }
        }
    }
}