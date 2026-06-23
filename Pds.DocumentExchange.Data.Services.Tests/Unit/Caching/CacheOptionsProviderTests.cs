using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Caching.Models;
using Pds.DocumentExchange.Data.Services.Implementations.Caching;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Caching
{
    [TestClass]
    public class CacheOptionsProviderTests
    {
        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Constructor_PerformsNullChecks(bool cacheConfigurationNull)
        {
            // Arrange
            var cacheConfiguration = cacheConfigurationNull ? null : Mock.Of<ICacheConfiguration>();

            // Act
            Action act = () => new CacheOptionsProvider(cacheConfiguration);

            // Assert
            if (cacheConfigurationNull)
            {
                act.Should().Throw<ArgumentNullException>();
            }
            else
            {
                act.Should().NotThrow();
            }
        }

        [TestMethod]
        [DataRow(true, true, true, 1, true)]
        [DataRow(true, false, true, 10, false)]
        [DataRow(false, true, false, 60, true)]
        public void DocumentList_PropertyGetters_ReturnExpectedValues(
              bool cacheNullData,
              bool useMemoryCache,
              bool useDistributedCache,
              int cacheTimeMins,
              bool useCompression)
        {
            // Arrange
            var cacheConfiguration = Mock.Of<ICacheConfiguration>(MockBehavior.Strict);
            var cacheConfigurationMock = Mock.Get(cacheConfiguration);

            cacheConfigurationMock
                .SetupGet(c => c.DocumentListCacheNullData)
                .Returns(cacheNullData);

            cacheConfigurationMock
               .SetupGet(c => c.DocumentListUseMemoryCache)
               .Returns(useMemoryCache);

            cacheConfigurationMock
               .SetupGet(c => c.DocumentListUseDistributedCache)
               .Returns(useDistributedCache);

            cacheConfigurationMock
                .SetupGet(c => c.DocumentListCacheTimeMinutes)
                .Returns(cacheTimeMins);

            cacheConfigurationMock
                .SetupGet(c => c.DocumentListUseCompression)
                .Returns(useCompression);

            var provider = new CacheOptionsProvider(cacheConfiguration);

            // Act
            var actualDocumentListCachingOptions = provider.DocumentList;

            // Assert
            actualDocumentListCachingOptions.Should().BeEquivalentTo(
                new CacheOptions
                {
                    CacheNullData = cacheNullData,
                    UseMemoryCache = useMemoryCache,
                    UseDistributedCache = useDistributedCache,
                    MemoryCacheTTL = TimeSpan.FromMinutes(cacheTimeMins),
                    DistributedCacheTTL = TimeSpan.FromMinutes(cacheTimeMins),
                    UseCompression = useCompression
                });

            cacheConfigurationMock.VerifyAll();
        }

        [TestMethod]
        [DataRow(true, true, true, 1, true)]
        [DataRow(true, false, true, 10, false)]
        [DataRow(false, true, false, 60, true)]
        public void DocumentValidation_PropertyGetters_ReturnExpectedValues(
               bool cacheNullData,
               bool useMemoryCache,
               bool useDistributedCache,
               int cacheTimeMins,
               bool useCompression)
        {
            // Arrange
            var cacheConfiguration = Mock.Of<ICacheConfiguration>(MockBehavior.Strict);
            var cacheConfigurationMock = Mock.Get(cacheConfiguration);

            cacheConfigurationMock
                .SetupGet(c => c.DocumentValidationCacheNullData)
                .Returns(cacheNullData);

            cacheConfigurationMock
               .SetupGet(c => c.DocumentValidationUseMemoryCache)
               .Returns(useMemoryCache);

            cacheConfigurationMock
               .SetupGet(c => c.DocumentValidationUseDistributedCache)
               .Returns(useDistributedCache);

            cacheConfigurationMock
                .SetupGet(c => c.DocumentValidationCacheTimeMinutes)
                .Returns(cacheTimeMins);

            cacheConfigurationMock
                .SetupGet(c => c.DocumentValidationUseCompression)
                .Returns(useCompression);

            var provider = new CacheOptionsProvider(cacheConfiguration);

            // Act
            var actualDocumentValidationCachingOptions = provider.DocumentValidation;

            // Assert
            actualDocumentValidationCachingOptions.Should().BeEquivalentTo(
                new CacheOptions
                {
                    CacheNullData = cacheNullData,
                    UseMemoryCache = useMemoryCache,
                    UseDistributedCache = useDistributedCache,
                    MemoryCacheTTL = TimeSpan.FromMinutes(cacheTimeMins),
                    DistributedCacheTTL = TimeSpan.FromMinutes(cacheTimeMins),
                    UseCompression = useCompression
                });

            cacheConfigurationMock.VerifyAll();
        }

        [TestMethod]
        [DataRow(true, true, true, 60, 1, true)]
        [DataRow(true, false, true, 100, 60, false)]
        [DataRow(false, true, false, 999, 999, true)]
        public void Configuration_PropertyGetters_ReturnExpectedValues(
               bool cacheNullData,
               bool useMemoryCache,
               bool useDistributedCache,
               int memoryCacheTimeSeconds,
               int distributedCacheTimeMinutes,
               bool useCompression)
        {
            // Arrange
            var cacheConfiguration = Mock.Of<ICacheConfiguration>(MockBehavior.Strict);
            var cacheConfigurationMock = Mock.Get(cacheConfiguration);

            cacheConfigurationMock
                .SetupGet(c => c.ConfigurationCacheNullData)
                .Returns(cacheNullData);

            cacheConfigurationMock
               .SetupGet(c => c.ConfigurationUseMemoryCache)
               .Returns(useMemoryCache);

            cacheConfigurationMock
               .SetupGet(c => c.ConfigurationUseDistributedCache)
               .Returns(useDistributedCache);

            cacheConfigurationMock
                .SetupGet(c => c.ConfigurationMemoryCacheTimeSeconds)
                .Returns(memoryCacheTimeSeconds);

            cacheConfigurationMock
                .SetupGet(c => c.ConfigurationDistributedCacheTimeMinutes)
                .Returns(distributedCacheTimeMinutes);

            cacheConfigurationMock
                .SetupGet(c => c.ConfigurationUseCompression)
                .Returns(useCompression);

            var provider = new CacheOptionsProvider(cacheConfiguration);

            // Act
            var actualConfigurationCachingOptions = provider.Configuration;

            // Assert
            actualConfigurationCachingOptions.Should().BeEquivalentTo(
                new CacheOptions
                {
                    CacheNullData = cacheNullData,
                    UseMemoryCache = useMemoryCache,
                    UseDistributedCache = useDistributedCache,
                    MemoryCacheTTL = TimeSpan.FromSeconds(memoryCacheTimeSeconds),
                    DistributedCacheTTL = TimeSpan.FromMinutes(distributedCacheTimeMinutes),
                    UseCompression = useCompression
                });

            cacheConfigurationMock.VerifyAll();
        }

        [TestMethod]
        [DataRow(true, true, true, 1, true)]
        [DataRow(true, false, true, 10, false)]
        [DataRow(false, true, false, 60, true)]
        public void OrganisationTypes_PropertyGetters_ReturnExpectedValues(
               bool cacheNullData,
               bool useMemoryCache,
               bool useDistributedCache,
               int cacheTimeDays,
               bool useCompression)
        {
            // Arrange
            var cacheConfiguration = Mock.Of<ICacheConfiguration>(MockBehavior.Strict);
            var cacheConfigurationMock = Mock.Get(cacheConfiguration);

            cacheConfigurationMock
                .SetupGet(c => c.OrganisationTypesCacheNullData)
                .Returns(cacheNullData);

            cacheConfigurationMock
               .SetupGet(c => c.OrganisationTypesUseMemoryCache)
               .Returns(useMemoryCache);

            cacheConfigurationMock
               .SetupGet(c => c.OrganisationTypesUseDistributedCache)
               .Returns(useDistributedCache);

            cacheConfigurationMock
                .SetupGet(c => c.OrganisationTypesCacheTimeDays)
                .Returns(cacheTimeDays);

            cacheConfigurationMock
                .SetupGet(c => c.OrganisationTypesUseCompression)
                .Returns(useCompression);

            var provider = new CacheOptionsProvider(cacheConfiguration);

            // Act
            var actualOrganisationTypesCachingOptions = provider.OrganisationTypes;

            // Assert
            actualOrganisationTypesCachingOptions.Should().BeEquivalentTo(
                new CacheOptions
                {
                    CacheNullData = cacheNullData,
                    UseMemoryCache = useMemoryCache,
                    UseDistributedCache = useDistributedCache,
                    MemoryCacheTTL = TimeSpan.FromDays(cacheTimeDays),
                    DistributedCacheTTL = TimeSpan.FromDays(cacheTimeDays),
                    UseCompression = useCompression
                });

            cacheConfigurationMock.VerifyAll();
        }
    }
}
